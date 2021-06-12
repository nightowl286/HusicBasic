using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using HusicBasic.Models;
using HusicBasic.Services;
using Prism.Commands;
using Prism.Mvvm;

namespace HusicBasic.ViewModels
{
    public class MainPlayerViewModel : BindableBase
    {
        #region Private
        private DelegateCommand<object> _SetVolume;
        private DelegateCommand<double?> _ForceSetVolume;
        private DelegateCommand<object> _SetPosition;
        private DelegateCommand _TogglePause;
        private DelegateCommand _PlayNext;
        private DelegateCommand _PlayPrevious;
        private DelegateCommand _ToggleMute;
        private DelegateCommand _CircleRepeatMode;
        private DelegateCommand _CircleShuffler;
        private DelegateCommand _ExitCommand;
        private DelegateCommand _ShowMainPlayerCommand;
        private DelegateCommand _ShowMiniPlayerCommand;
        private string _TogglePauseText;
        private string _Title;
        private string _SongName;
        private string _SongNameSafe = "No song currently playing";
        private readonly IHusicSettings Settings;
        private string _MuteText;
        private bool _IsDefaultRepeatMode;
        private bool _IsDefaultShuffler;
        private string _RepeatModeText;
        private string _RepeatModeName;
        private string _ShufflerIcon;
        private string _ShufflerName;
        private string _YoutubeID;
        private string _YoutubeLink;
        private FontFamily _ShufflerIconFont;
        private SongModel _LastSong;
        #endregion

        #region Properties
        private IHusicPlayer Player { get; set; }
        public bool IsPlaying => Player.IsPlaying;
        public bool IsLoaded => Player.IsLoaded;
        public double Volume { get => Player.Volume; set { Player.Volume = value; RaisePropertyChanged(); } }
        public DelegateCommand<object> SetVolume { get => _SetVolume; private set => SetProperty(ref _SetVolume, value); }
        public DelegateCommand<object> SetPosition { get => _SetPosition; private set => SetProperty(ref _SetPosition, value); }
        public DelegateCommand<double?> ForceSetVolume { get => _ForceSetVolume; private set => SetProperty(ref _ForceSetVolume, value); }
        public DelegateCommand TogglePause { get => _TogglePause; private set => SetProperty(ref _TogglePause, value); }
        public DelegateCommand PlayNext { get => _PlayNext; private set => SetProperty(ref _PlayNext, value); }
        public DelegateCommand PlayPrevious { get => _PlayPrevious; private set => SetProperty(ref _PlayPrevious, value); }
        public DelegateCommand ToggleMute { get => _ToggleMute; private set => SetProperty(ref _ToggleMute, value); }
        public DelegateCommand CircleRepeatMode { get => _CircleRepeatMode; private set => SetProperty(ref _CircleRepeatMode, value); }
        public DelegateCommand CircleShuffler { get => _CircleShuffler; private set => SetProperty(ref _CircleShuffler, value); }
        public DelegateCommand ExitCommand { get => _ExitCommand; private set => SetProperty(ref _ExitCommand, value); }
        public DelegateCommand ShowMainPlayerCommand { get => _ShowMainPlayerCommand; private set => SetProperty(ref _ShowMainPlayerCommand, value); }
        public DelegateCommand ShowMiniPlayerCommand { get => _ShowMiniPlayerCommand; private set => SetProperty(ref _ShowMiniPlayerCommand, value); }
        public string TogglePauseText { get => _TogglePauseText; private set => SetProperty(ref _TogglePauseText, value); }
        public string MuteText { get => _MuteText; private set => SetProperty(ref _MuteText, value); }
        public string YoutubeID { get => _YoutubeID; private set => SetProperty(ref _YoutubeID, value, YoutubeIdChanged); }
        public string YoutubeLink { get => _YoutubeLink; private set => SetProperty(ref _YoutubeLink, value); }
        public double Position => IsLoaded ? Player.Position.TotalSeconds : 0;
        public double Duration => IsLoaded ? Player.Duration.TotalSeconds : 0;
        public string PositionText => IsLoaded ? TimeSpan.FromSeconds(Position).FormatTime() : "00:00";
        public string DurationText => IsLoaded ? Player.Duration.FormatTime() : "00:00";
        public string TimeLeftText { get {
                if (IsLoaded)
                {
                    TimeSpan d = Player.Duration;
                    TimeSpan p = TimeSpan.FromSeconds(Position);
                    TimeSpan t = d.Subtract(p);
                    return t.FormatTime(true);
                        }
                else return "-00:00"; } }
        public string Title { get => _Title; private set => SetProperty(ref _Title, value); }
        public string SongName { get => _SongName; private set => SetProperty(ref _SongName, value, SongNameChanged); }
        public string SongNameSafe { get => _SongNameSafe; private set => SetProperty(ref _SongNameSafe, value); }
        public bool IsDefaultRepeatMode { get => _IsDefaultRepeatMode; private set => SetProperty(ref _IsDefaultRepeatMode, value); }
        public bool IsDefaultShuffler { get => _IsDefaultShuffler; private set => SetProperty(ref _IsDefaultShuffler, value); }
        public string RepeatModeText { get => _RepeatModeText; private set => SetProperty(ref _RepeatModeText, value); }
        public string RepeatModeName { get => _RepeatModeName; private set => SetProperty(ref _RepeatModeName, value); }
        public string ShufflerIcon { get => _ShufflerIcon; private set => SetProperty(ref _ShufflerIcon, value); }
        public string ShufflerName { get => _ShufflerName; private set => SetProperty(ref _ShufflerName, value); }
        public FontFamily ShufflerIconFont { get => _ShufflerIconFont; private set => SetProperty(ref _ShufflerIconFont, value); }
        #endregion
        public MainPlayerViewModel(IHusicPlayer player, IHusicSettings settings)
        {
            Player = player;
            Settings = settings;
            CreateCommands();

            UpdateAllInfo();

            ExitCommand = new DelegateCommand(GlobalCommands.Exit.Execute);
            ShowMiniPlayerCommand = new DelegateCommand(GlobalCommands.ShowMiniPlayer.Execute);
            ShowMainPlayerCommand = new DelegateCommand(GlobalCommands.ShowMainPlayer.Execute);

            player.PropertyChanged += Player_PropertyChanged;
        }

        #region Methods
        private void SongNameChanged()
        {
            if (string.IsNullOrWhiteSpace(SongName))
                SongNameSafe = "No song currently playing";
            else
                SongNameSafe = SongName;
        }
        private void YoutubeIdChanged() => YoutubeLink = $"https://www.youtube.com/watch?v={YoutubeID}";
        private void UpdateAllInfo()
        {
            RaisePropertyChanged(nameof(IsPlaying));
            UpdateYoutubeID();
            UpdatePlayPauseText();
            UpdatePositionAndDuration();
            UpdateMuteText();
            UpdateSongName();
            UpdateRepeatMode();
            UpdateShuffler();
        }
        private void UpdateYoutubeID()
        {
            if (string.IsNullOrWhiteSpace(Player.CurrentSong?.YoutubeID))
                YoutubeID = "";
            else
                YoutubeID = Player.CurrentSong.YoutubeID;
        }
        private void UpdateMuteText()
        {
            string resName;
            if (Player.IsMuted) resName = "Text.Muted";
            else
            {
                resName = "Text.Volume.";
                if (Volume >= 0.66) resName += "High";
                else if (Volume >= 0.33) resName += "Mid";
                else if (Volume > 0) resName += "Low";
                else resName += "None";
            }
            MuteText = App.Current.FindResource(resName) as string;
        }
        private void UpdateSongName()
        {
            if (Player.IsLoaded)
                SongName = Player.CurrentSong.Title;
            else
                SongName = null;

            if (Player.IsPlaying)
                Title = $"Husic mini | Playing: {SongName}";
            else
                Title = "Husic mini";

        }
        private void UpdatePositionAndDuration()
        {
            UpdatePosition();
            UpdateDuration();
        }
        private void UpdateDuration()
        {
            RaisePropertyChanged(nameof(Duration));
            RaisePropertyChanged(nameof(DurationText));
            RaisePropertyChanged(nameof(TimeLeftText));
        }
        private void UpdatePosition()
        {
            RaisePropertyChanged(nameof(Position));
            RaisePropertyChanged(nameof(PositionText));
            RaisePropertyChanged(nameof(TimeLeftText));
        }
        private void UpdatePlayPauseText() => TogglePauseText = App.Current.FindResource(((Player.IsPaused && Player.IsPlaying) || (Player.IsLoaded && !Player.IsPlaying)) ? "Text.Play" : "Text.Pause") as string;
        private void CreateCommands()
        {
            SetVolume = new DelegateCommand<object>(ExecuteSetVolume);
            SetPosition = new DelegateCommand<object>(ExecuteSetPosition).ObservesCanExecute(() => Player.IsLoaded);
            ForceSetVolume = new DelegateCommand<double?>(ForceSetVolumeCallback);
            TogglePause = new DelegateCommand(Player.TogglePause).ObservesCanExecute(() => Player.IsLoaded);
            PlayNext = new DelegateCommand(Player.PlayNext).ObservesCanExecute(() => Player.CanPlayNext);
            PlayPrevious = new DelegateCommand(Player.PlayPrevious).ObservesCanExecute(() => Player.CanPlayPrevious);
            ToggleMute = new DelegateCommand(Player.ToggleMute);
            CircleRepeatMode = new DelegateCommand(Player.CircleRepeatMode);
            CircleShuffler = new DelegateCommand(Player.CircleShuffler);
        }
        private void ExecuteSetPosition(object source)
        {
            if (source is Slider slid && slid.IsMouseOver)
            {
                double newPos = slid.Value;
                if ((SystemParameters.SwapButtons ? Mouse.RightButton : Mouse.LeftButton) == MouseButtonState.Pressed)
                    Player.Position = TimeSpan.FromSeconds(newPos);
            }
        }
        private void ExecuteSetVolume(object source)
        {
            if (source is Slider slid && slid.IsMouseOver)
            {
                if ((SystemParameters.SwapButtons ? Mouse.RightButton : Mouse.LeftButton) == MouseButtonState.Pressed)
                    Player.Volume = slid.Value / 100d;
            }
        }
        private void ForceSetVolumeCallback(double? volume)
        {
            if (volume.HasValue)
            {
                Dispatcher.CurrentDispatcher.Invoke(() => {

                    Player.Volume = volume.Value / 100d;
                });
            }
        }
        private void UpdateRepeatMode()
        {
            IsDefaultRepeatMode = Player.Repeat == RepeatMode.Dont;
            RepeatModeText = App.Current.FindResource($"Text.Repeat.{Player.Repeat}") as string;

            if (Player.Repeat == RepeatMode.Dont)
                RepeatModeName = "Don't repeat";
            else
                RepeatModeName = $"Repeat {Player.Repeat.ToString().ToLower()}";

        }
        private void UpdateShuffler()
        {
            IsDefaultShuffler = Player.Shuffler is LinearShuffler;
            ShufflerIcon = Player.Shuffler.Icon;
            ShufflerIconFont = Player.Shuffler.IconFont;
            ShufflerName = Player.Shuffler.Name;
        }
        private void CurrentSong_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SongModel.Duration)) UpdateDuration();
            else if (e.PropertyName == nameof(SongModel.YoutubeID)) UpdateYoutubeID();
            else if (e.PropertyName == nameof(SongModel.Title)) UpdateSongName();

        }
        private void Player_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IHusicPlayer.IsPaused)) UpdatePlayPauseText();
            else if (e.PropertyName == nameof(IHusicPlayer.Position)) UpdatePosition();
            else if (e.PropertyName == nameof(IHusicPlayer.Duration)) UpdateDuration();
            else if (e.PropertyName == nameof(IHusicPlayer.CurrentSong))
            {
                if (_LastSong != null)
                    _LastSong.PropertyChanged -= CurrentSong_PropertyChanged;

                _LastSong = Player.CurrentSong;
                _LastSong.PropertyChanged += CurrentSong_PropertyChanged;

                UpdateSongName();
                UpdateYoutubeID();
            }
            else if (e.PropertyName == nameof(IHusicPlayer.IsPlaying))
            {
                RaisePropertyChanged(nameof(IsPlaying));
                UpdatePositionAndDuration();
                UpdateSongName();
                UpdatePlayPauseText();
            }
            else if (e.PropertyName == nameof(IHusicPlayer.Volume))
            {
                RaisePropertyChanged(nameof(Volume));
                UpdateMuteText();
            }
            else if (e.PropertyName == nameof(IHusicPlayer.IsMuted)) UpdateMuteText();
            else if (e.PropertyName == nameof(IHusicPlayer.IsLoaded))
            {
                RaisePropertyChanged(nameof(IsLoaded));
                UpdatePositionAndDuration();
                UpdateSongName();
                UpdatePlayPauseText();
            }
            else if (e.PropertyName == nameof(IHusicPlayer.Repeat)) UpdateRepeatMode();
            else if (e.PropertyName == nameof(IHusicPlayer.Shuffler)) UpdateShuffler();
            else RaisePropertyChanged(e.PropertyName);
        }
        #endregion
    }
}
