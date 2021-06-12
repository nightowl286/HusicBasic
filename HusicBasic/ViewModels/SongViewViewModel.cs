using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using HusicBasic.Models;
using HusicBasic.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace HusicBasic.ViewModels
{
    public class SongViewViewModel : BindableBase
    {
        #region Private
        private SongModel _OldSong;
        private SongModel _Song;
        private string _Title;
        private string _ID;
        private string _Location;
        private string _Duration;
        private string _YoutubeID;
        private DelegateCommand _PlayCommand;
        private DelegateCommand _PlayNextCommand;
        private DelegateCommand _AddQueueCommand;
        private DelegateCommand<SongModel> _ShowOverviewCommand;
        private readonly IHusicPlayer Player;
        #endregion

        #region Properties
        public SongModel Song { get => _Song; set => SetProperty(ref _Song, value, SongChanged); }
        public string Title { get => _Title; private set => SetProperty(ref _Title, value); }
        public string ID { get => _ID; private set => SetProperty(ref _ID, value); }
        public string Location { get => _Location; private set => SetProperty(ref _Location, value); }
        public string Duration { get => _Duration; private set => SetProperty(ref _Duration, value); }
        public DelegateCommand PlayCommand { get => _PlayCommand; private set => SetProperty(ref _PlayCommand, value); }
        public DelegateCommand PlayNextCommand { get => _PlayNextCommand; private set => SetProperty(ref _PlayNextCommand, value); }
        public DelegateCommand AddQueueCommand { get => _AddQueueCommand; private set => SetProperty(ref _AddQueueCommand, value); }
        public DelegateCommand<SongModel> ShowOverviewCommand { get => _ShowOverviewCommand; private set => SetProperty(ref _ShowOverviewCommand, value); }
        public string YoutubeID { get => _YoutubeID; private set => SetProperty(ref _YoutubeID, value); }
        public string YoutubeLink => $"https://www.youtube.com/watch?v={YoutubeID}";
        #endregion
        public SongViewViewModel(SongModel song, IHusicPlayer player, DelegateCommand<SongModel> showOverviewCommand)
        {
            Song = song;
            Player = player;

            PlayCommand = new DelegateCommand(ExecutePlay);
            PlayNextCommand = new DelegateCommand(ExecutePlayNext);
            AddQueueCommand = new DelegateCommand(ExecuteAddQueue);
            ShowOverviewCommand = showOverviewCommand;
        }

        #region Events
        private void UpdateTitle() => Title = Song.Title;
        private void UpdateID() => ID = $"#{Song.ID}";
        private void UpdateDuration() => Duration = Song.Duration.FormatTime();
        private void UpdateLocation() => Location = Song.Path.LocalPath;
        private void UpdateYoutubeID()
        {
            YoutubeID = Song.YoutubeID;
            RaisePropertyChanged(nameof(YoutubeLink));
        }
        private void Song_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SongModel.ID)) UpdateID();
            else if (e.PropertyName == nameof(SongModel.Title)) UpdateTitle();
            else if (e.PropertyName == nameof(SongModel.Duration)) UpdateDuration();
            else if (e.PropertyName == nameof(SongModel.Path)) UpdateLocation();
            else if (e.PropertyName == nameof(SongModel.YoutubeID)) UpdateYoutubeID();
        }
        private void SongChanged()
        {
            if (_OldSong != null) _OldSong.PropertyChanged -= Song_PropertyChanged;
            _OldSong = Song;
            if (Song != null)
            {
                Song.PropertyChanged += Song_PropertyChanged;
                UpdateTitle();
                UpdateID();
                UpdateDuration();
                UpdateLocation();
                UpdateYoutubeID();
            }

        }
        #endregion

        #region Methods
        private void ExecutePlay() => Player.SwitchPlaybackTo(Song);
        private void ExecutePlayNext() => Player.Queue.InsertInNext(Song);
        private void ExecuteAddQueue() => Player.Queue.AddToRepeating(Song);
        #endregion
    }
}
