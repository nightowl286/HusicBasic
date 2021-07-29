using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using HusicBasic.Models;
using Prism.Mvvm;
using Prism.Regions;
using HusicBasic;
using System.Linq;
using Prism.Commands;
using HusicBasic.Services;
using Prism.Services.Dialogs;

namespace HusicBasic.ViewModels
{
    public class PlaylistOverviewViewModel : BindableBase, INavigationAware
    {
        #region Private
        private PlaylistModel _Playlist;
        private ObservableCollection<SongViewViewModel> _DisplaySongs;
        public string _Duration;
        public string _AverageDuration;
        private bool _NewNameValid;
        private string _CurrentName;
        private string _NewName;
        private IRegionNavigationJournal _Journal;
        private DelegateCommand _GoBackCommand;
        private DelegateCommand _DeleteCommand;
        private DelegateCommand _SaveCommand;
        private IPlaylistStore PlaylistStore;
        private IDialogService DialogService;
        #endregion

        #region Properties
        public PlaylistModel Playlist { get => _Playlist; private set => SetProperty(ref _Playlist, value, PlaylistChanged); }
        public ObservableCollection<SongViewViewModel> DisplaySongs { get => _DisplaySongs; private set => SetProperty(ref _DisplaySongs, value); }
        public string Duration { get => _Duration; private set => SetProperty(ref _Duration, value); }
        public string AverageDuration { get => _AverageDuration; private set => SetProperty(ref _AverageDuration, value); }
        public string CurrentName { get => _CurrentName; private set => SetProperty(ref _CurrentName, value); }
        public string NewName { get => _NewName; set => SetProperty(ref _NewName, value, NewNameChanged); }
        public bool NewNameValid { get => _NewNameValid; private set => SetProperty(ref _NewNameValid, value); }
        public IRegionNavigationJournal Journal { get => _Journal; private set => SetProperty(ref _Journal, value); }
        public DelegateCommand GoBackCommand { get => _GoBackCommand; private set => SetProperty(ref _GoBackCommand, value); }
        public DelegateCommand DeleteCommand { get => _DeleteCommand; private set => SetProperty(ref _DeleteCommand, value); }
        public DelegateCommand SaveCommand { get => _SaveCommand; private set => SetProperty(ref _SaveCommand, value); }
        #endregion
        public PlaylistOverviewViewModel(IPlaylistStore playlistStore, IDialogService dialogService)
        {
            GoBackCommand = new DelegateCommand(() => Journal.GoBack());
            DeleteCommand = new DelegateCommand(AskDeletion);
            SaveCommand = new DelegateCommand(Save);
            PlaylistStore = playlistStore;
            DialogService = dialogService;
        }

        #region Methods
        private void AskDeletion() => DialogService.ConfirmDeletion(DeletionCallback, Playlist);
        private void Save()
        {

        }
        private void DeletionCallback(IDialogResult result)
        {
            if (result.Result == ButtonResult.Yes)
            {
                PlaylistStore.Remove(Playlist);
                GoBackCommand.Execute();
            }
        }
        private void NewNameChanged()
        {
            string trimmed = NewName.Trim();
            NewNameValid = (NewName.Length == 0 || trimmed.Equals(CurrentName) || !PlaylistStore.ContainsByName(trimmed));
        }
        private void PlaylistChanged()
        {
            Duration = Playlist.Duration.FormatNice();
            AverageDuration = Playlist.GetAvgDuration().FormatNice();
            DisplaySongs = new ObservableCollection<SongViewViewModel>(Playlist.Songs.Select(s => new SongViewViewModel(s, null,null)));
            CurrentName = Playlist.Title;
            NewName = CurrentName;
        }
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Journal = navigationContext.NavigationService.Journal;
            if (navigationContext.Parameters.TryGetValue("playlist", out PlaylistModel playlist))
                Playlist = playlist;
        }
        public bool IsNavigationTarget(NavigationContext navigationContext) => true;
        public void OnNavigatedFrom(NavigationContext navigationContext) { }
        #endregion
    }
}
