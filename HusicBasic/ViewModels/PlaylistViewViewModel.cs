using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using HusicBasic.Models;
using HusicBasic.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace HusicBasic.ViewModels
{
    public class PlaylistViewViewModel : BindableBase
    {
        #region Private
        private PlaylistModel _Playlist;
        private PlaylistModel _LastPlaylist;
        private string _Duration;
        private string _AverageDuration;
        private string _Name;
        private int _SongCount;
        private uint _ID;
        private DelegateCommand _PlayCommand;
        private DelegateCommand _PlayNextCommand;
        private DelegateCommand _AddQueueCommand;
        private DelegateCommand<PlaylistModel> _ShowOverviewCommand;
        private readonly IHusicPlayer Player;
        #endregion

        #region Properties
        public PlaylistModel Playlist { get => _Playlist; set => SetProperty(ref _Playlist, value, PlaylistChanged); }
        public uint ID { get => _ID; private set => SetProperty(ref _ID, value); }
        public string AverageDuration { get => _AverageDuration; private set => SetProperty(ref _AverageDuration, value); }
        public string Duration { get => _Duration; private set => SetProperty(ref _Duration, value); }
        public int SongCount { get => _SongCount; private set => SetProperty(ref _SongCount, value); }
        public string Name { get => _Name; private set => SetProperty(ref _Name, value); }
        public DelegateCommand PlayCommand { get => _PlayCommand; private set => SetProperty(ref _PlayCommand, value); }
        public DelegateCommand PlayNextCommand { get => _PlayNextCommand; private set => SetProperty(ref _PlayNextCommand, value); }
        public DelegateCommand AddQueueCommand { get => _AddQueueCommand; private set => SetProperty(ref _AddQueueCommand, value); }
        public DelegateCommand<PlaylistModel> ShowOverviewCommand { get => _ShowOverviewCommand; private set => SetProperty(ref _ShowOverviewCommand, value); }
        #endregion
        public PlaylistViewViewModel(PlaylistModel playlist, IHusicPlayer player, DelegateCommand<PlaylistModel> showPlaylistOverview)
        {
            Player = player;
            Playlist = playlist;

            PlayCommand = new DelegateCommand(ExecutePlay);
            PlayNextCommand = new DelegateCommand(ExecutePlayNext);
            AddQueueCommand = new DelegateCommand(ExecuteAddQueue);
            ShowOverviewCommand = showPlaylistOverview;
        }

        #region Events
        private void Playlist_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlaylistModel.ID)) ID = Playlist.ID;
            else if (e.PropertyName == nameof(PlaylistModel.Duration))
            {
                Duration = Playlist.Duration.FormatNice();
                UpdateAverageDuration();
            }
            else if (e.PropertyName == nameof(PlaylistModel.Title)) Name = Playlist.Title;
            else if (e.PropertyName == nameof(PlaylistModel.Count)) SongCount = Playlist.Count;
        }
        #endregion

        #region Methods
        private void PlaylistChanged()
        {
            if (_LastPlaylist != null)
                _LastPlaylist.PropertyChanged -= Playlist_PropertyChanged;

            _LastPlaylist = Playlist;
            _LastPlaylist.PropertyChanged += Playlist_PropertyChanged;

            ID = Playlist.ID;
            Duration = Playlist.Duration.FormatNice();
            UpdateAverageDuration();
            Name = Playlist.Title;
            SongCount = Playlist.Count;
        }
        private void UpdateAverageDuration()
        {
            List<double> durations = new List<double>(Playlist.Songs.Select(s => s.Duration.TotalSeconds));
            double correctedAverage = durations.GetCorrectedAverage();
            AverageDuration = TimeSpan.FromSeconds(correctedAverage).FormatNice();
        }
        private void ExecutePlay()
        {
            Player.Queue.Clear(QueueSource.Queue);
            Player.Queue.AddToRepeating(Playlist.Songs);
            Player.Queue.Clear(QueueSource.Next);
            Player.PlayNext();
        }
        private void ExecutePlayNext() => Player.Queue.InsertInNext(Playlist.Songs);
        private void ExecuteAddQueue() => Player.Queue.AddToRepeating(Playlist.Songs);
        #endregion
    }
}
