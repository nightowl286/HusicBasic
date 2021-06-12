using System;
using System.Collections.Generic;
using System.Linq;
using HusicBasic.Models;
using HusicBasic.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace HusicBasic.ViewModels
{
    public class SongOverviewViewModel : BindableBase, INavigationAware
    {
        #region Private
        private bool _NewNameValid;
        private string _CurrentName;
        private string _NewName;
        private string _Duration;
        private string _Source;
        private string _YoutubeID;
        private SongModel _Song;
        private DelegateCommand _GoBackCommand;
        private DelegateCommand _SaveCommand;
        private IRegionNavigationJournal _Journal;
        private readonly ISongStore SongStore;
        #endregion

        #region Properties
        public bool NewNameValid { get => _NewNameValid; private set => SetProperty(ref _NewNameValid, value); }
        public string CurrentName { get => _CurrentName; private set => SetProperty(ref _CurrentName, value); }
        public string NewName { get => _NewName; set => SetProperty(ref _NewName, value, NewNameChanged); }
        public string Duration { get => _Duration; set => SetProperty(ref _Duration, value); }
        public string Source { get => _Source; set => SetProperty(ref _Source, value); }
        public string YoutubeID { get => _YoutubeID; set => SetProperty(ref _YoutubeID, value); }
        public SongModel Song { get => _Song; private set => SetProperty(ref _Song, value); }
        public DelegateCommand GoBackCommand { get => _GoBackCommand; private set => SetProperty(ref _GoBackCommand, value); }
        public DelegateCommand SaveCommand { get => _SaveCommand; private set => SetProperty(ref _SaveCommand, value); }
        public IRegionNavigationJournal Journal { get => _Journal; private set => SetProperty(ref _Journal, value); }
        #endregion
        public SongOverviewViewModel(ISongStore songStore)
        {
            SongStore = songStore;
            SaveCommand = new DelegateCommand(Save).ObservesCanExecute(() => NewNameValid);
        }

        #region Methods
        private void NewNameChanged()
        {
            string trimmed = NewName.Trim();
            NewNameValid = (NewName.Length == 0 || trimmed.Equals(CurrentName) || !SongStore.ContainsByName(trimmed));
        }
        private void Save()
        {
            string trimmed = NewName.Trim();
            if (trimmed.Length > 0 && !trimmed.Equals(CurrentName))
            {
                Song.Title = trimmed;
                Song.Save();
            }
            GoBackCommand.Execute();
        }
        public bool IsNavigationTarget(NavigationContext navigationContext) => false;
        public void OnNavigatedFrom(NavigationContext navigationContext) {}
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Journal = navigationContext.NavigationService.Journal;
            if (navigationContext.Parameters.TryGetValue("song", out SongModel song))
            {
                GoBackCommand = new DelegateCommand(() => Journal.GoBack());
                Song = song;
                CurrentName = song.Title;
                NewName = CurrentName;
                Source = song.Path.LocalPath;
                Duration = song.Duration.FormatNice();
                NewNameValid = true;
                YoutubeID = string.IsNullOrWhiteSpace(song.YoutubeID) ? "" : song.YoutubeID;
            }
        }
        #endregion
    }
}
