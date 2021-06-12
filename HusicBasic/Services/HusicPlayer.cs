using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using HusicBasic.Events;
using HusicBasic.Models;
using Prism.Events;
using Prism.Mvvm;

namespace HusicBasic.Services
{
    public class HusicPlayer : BindableBase, IHusicPlayer
    {
        #region Private
        private readonly MediaPlayer _Player;
        private TimeSpan _CurrentDuration;
        private bool _IsPaused;
        private SongModel _CurrentSong;
        private bool _IsPlaying;
        private readonly DispatcherTimer _PositionTimer;
        private readonly IHusicSettings Settings;
        private bool _IsLoaded;
        private RepeatMode _Repeat;
        private IPlayQueue _Queue;
        private ISongShuffler _Shuffler;
        private readonly List<ISongShuffler> AllShufflers;
        private int _CurrentShufflerIndex;
        private TimeSpan _LastPosition;
        private bool _AllowEarlierPosition = false;
        private readonly SongRemovedEvent SongRemoved;
        #endregion

        #region Properties
        public double Volume { get => _Player.Volume; set { _Player.Volume = value; VolumeChanged(); } }
        public double Balance { get => _Player.Balance; set { _Player.Balance = value; RaisePropertyChanged(); } }
        public TimeSpan Position { get {

                TimeSpan t = _Player.Position;

                if (t < _LastPosition)
                {
                    if (_AllowEarlierPosition)
                        _LastPosition = t;
                }
                else 
                    _LastPosition = t;
                _AllowEarlierPosition = false;
                return _LastPosition;

            } set {
                _AllowEarlierPosition = true;
                PositionChangedSetter(value);
            }
        }
        public TimeSpan Duration { get => _CurrentDuration; private set => SetProperty(ref _CurrentDuration, value); }
        public bool IsMuted { get => _Player.IsMuted; set { _Player.IsMuted = value; RaisePropertyChanged(); } }
        public bool IsPaused { get => _IsPaused; set => SetProperty(ref _IsPaused, value, IsPausedChanged); }
        public SongModel CurrentSong { get => _CurrentSong; private set => SetProperty(ref _CurrentSong, value, SongChanged); }
        public bool IsPlaying { get => _IsPlaying; private set => SetProperty(ref _IsPlaying, value, UpdatePositionAndDurationInfo); }
        public bool IsLoaded { get => _IsLoaded; private set => SetProperty(ref _IsLoaded, value); }
        public bool CanPlayPrevious => Queue.CanGetPrevious;
        public bool CanPlayNext => (Repeat == RepeatMode.All && (Queue.Repeating.Count > 0 || (Queue.Next.Count > 0 || Queue.Queue.Count > 0))) || Queue.CanGetNext;
        public RepeatMode Repeat { get => _Repeat; set => SetProperty(ref _Repeat, value); }
        public IPlayQueue Queue { get => _Queue; private set => SetProperty(ref _Queue, value); }
        public ISongShuffler Shuffler { get => _Shuffler; set => SetProperty(ref _Shuffler, value, ShufflerChanged); }
        #endregion
        public HusicPlayer(IHusicSettings settings, IPlayQueue queue, IEnumerable<ISongShuffler> shufflers, IEventAggregator events)
        {
            SongRemoved = events.GetEvent<SongRemovedEvent>();
            SongRemoved.Subscribe(SongRemovedCallback, s => CurrentSong == s);
            _Player = new MediaPlayer();
            _Player.MediaOpened += _Player_MediaOpened;
            _Player.MediaFailed += _Player_MediaFailed;
            _Player.MediaEnded += _Player_MediaEnded;
            _PositionTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(125) };
            _PositionTimer.Tick += _PositionTimer_Tick;

            Queue = queue;
            queue.Player = this;
            queue.SongRequested += Queue_SongRequested;

            AllShufflers = shufflers.ToList();
            Debug.Assert(AllShufflers.Count > 0);
            _CurrentShufflerIndex = 0;
            Shuffler = AllShufflers[0];

            Repeat = RepeatMode.All;
            Queue.PropertyChanged += Queue_PropertyChanged;

            Settings = settings;
            _Player.Volume = settings.LastVolume;
        }

        #region Events
        private void SongRemovedCallback(SongModel song)
        {
            Queue.RemoveSong(song);
            _Player.Stop();
            IsLoaded = false;
            IsPlaying = false;
            PlayNext();
        }
        private void Queue_SongRequested(IPlayQueue queue, SongModel song)
        {
            PlaySong(song);

            if (!Queue.CanGetNext && Repeat == RepeatMode.All)
                Queue.AppendRepeating();
        }
        private void ShufflerChanged()
        {
            if (Queue != null)
            {
                Queue.Shuffler = Shuffler;
                Queue.ReshuffleQueue();
            }
        }
        private void Queue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IPlayQueue.CanGetNext)) RaisePropertyChanged(nameof(CanPlayNext));
            if (e.PropertyName == nameof(IPlayQueue.CanGetPrevious)) RaisePropertyChanged(nameof(CanPlayPrevious));
            
        }
        private void VolumeChanged()
        {
            RaisePropertyChanged(nameof(Volume));
            Settings.LastVolume = Volume;
        }
        private void SongChanged()
        {
            UpdatePositionAndDurationInfo();
        }
        private void _PositionTimer_Tick(object sender, EventArgs e) => RaisePropertyChanged(nameof(Position));
        private void _Player_MediaEnded(object sender, EventArgs e)
        {
            _AllowEarlierPosition = true;
            IsPlaying = false;
            _PositionTimer.Stop();

            if (Repeat == RepeatMode.Current)
            {
                Position = TimeSpan.Zero;
                _Player.Play();
                IsPlaying = true;
                UpdatePositionAndDurationInfo();
            }
            else
                PlayNext();
        }
        private void _Player_MediaFailed(object sender, ExceptionEventArgs e)
        {
            Debug.WriteLine($"[Player] : Failed : {e.ErrorException}");
            _PositionTimer.Stop();
            IsPlaying = false;
            IsLoaded = false;
        }
        private void _Player_MediaOpened(object sender, EventArgs e)
        {
            _AllowEarlierPosition = true;
            _PositionTimer.Start();
            IsPaused = false;
            IsPlaying = true;
            IsLoaded = true;

            while (_Player.NaturalDuration == System.Windows.Duration.Automatic)
                Task.Delay(50).Wait();

            Duration = _Player.NaturalDuration.TimeSpan;
            if (CurrentSong.Duration.CompareTo(Duration) != 0)
                CurrentSong.UpdateDuration(Duration);
        }
        #endregion

        #region Methods
        private void PositionChangedSetter(TimeSpan position)
        {
            if (IsPlaying || IsLoaded)
                _Player.Position = position;

            if (!IsPlaying)
            {
                IsPlaying = true;
                _Player.Play();
            }

            UpdatePositionAndDurationInfo();
        }
        private void UpdatePositionAndDurationInfo()
        {
            _PositionTimer.IsEnabled = (IsPlaying && !IsPaused);
            RaisePropertyChanged(nameof(Position));
            RaisePropertyChanged(nameof(Duration));
        }
        private void IsPausedChanged()
        {
            if (IsPaused)
                _Player.Pause();
            else
            {
                _Player.Play();
                IsPlaying = true;
            }
            UpdatePositionAndDurationInfo();
        }
        public void SwitchPlaybackTo(SongModel song)
        {
            Queue.MoveCurrentToHistory();
            Queue.SetCurrent(song);
            PlaySong(song);
        }
        public void Play(PlaylistModel playlist)
        {
            Queue.MoveCurrentToHistory();
            Queue.SetRepeating(playlist.Songs);
            PlayNext();
        }
        public void PlaySong(SongModel song)
        {
            _Player.Open(song.Path);
            _Player.Play();
            Duration = song.Duration;
            CurrentSong = song;

            IsPlaying = true;
            IsPaused = false;
            UpdatePositionAndDurationInfo();
        }
        public void PlayNext()
        {
            if (Queue.Position == 0 && IsLoaded)
                Queue.MoveCurrentToHistory();

            SongModel song = null;
            bool wasFromHistory = Queue.PlayingFromHistory;
            if (Queue.CanGetNext)
                song = Queue.GetNext();
            else if (Repeat == RepeatMode.All && Queue.Repeating.Count > 0)
            {
                Queue.AppendRepeating();
                song = Queue.GetNext();
            }

            if (song != null)
            {
                PlaySong(song);
                if (!wasFromHistory)
                    Queue.SetCurrent(song);

                if (!Queue.CanGetNext && Repeat == RepeatMode.All && Queue.Repeating.Count > 0)
                    Queue.AppendRepeating();
            }

        }
        public void PlayPrevious()
        {
            PlaySong(Queue.GetPrevious());
        }
        public void ToggleMute() => IsMuted = !IsMuted;
        public void TogglePause()
        {
            if (IsPlaying && Position < Duration)
                IsPaused = !IsPaused;
            else
            {
                IsPlaying = true;
                IsPaused = false;
                Position = TimeSpan.Zero;
                _Player.Play();
                if (Queue.Current == null) Queue.SetCurrent(CurrentSong);
                UpdatePositionAndDurationInfo();
            }
        }
        public void Mute() => IsMuted = true;
        public void Unmute() => IsMuted = false;
        public void Pause() => IsPaused = true;
        public void Unpause() => IsPaused = false;
        public void CircleRepeatMode() => Repeat = (RepeatMode)((((int)Repeat) + 1) % (((int)RepeatMode.All) + 1));
        public void CircleShuffler()
        {
            _CurrentShufflerIndex = (_CurrentShufflerIndex + 1) % (AllShufflers.Count);
            Shuffler = AllShufflers[_CurrentShufflerIndex];
        }
        #endregion
    }
}
