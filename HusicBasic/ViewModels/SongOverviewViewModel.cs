using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using HusicBasic.Models;
using HusicBasic.Services;
using HusicBasic.Views;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;

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
        private DelegateCommand _DeleteSongCommand;
        private DelegateCommand _ChangeSourceCommand;
        private IRegionNavigationJournal _Journal;
        private readonly ISongStore SongStore;
        private readonly IDialogService DialogService;
        private readonly IDurationResolver DurationResolver;
        private bool _SourceChanged;
        private TimeSpan _NewDuration;
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
        public DelegateCommand DeleteSongCommand { get => _DeleteSongCommand; private set => SetProperty(ref _DeleteSongCommand, value); }
        public DelegateCommand ChangeSourceCommand { get => _ChangeSourceCommand; private set => SetProperty(ref _ChangeSourceCommand, value); }
        public IRegionNavigationJournal Journal { get => _Journal; private set => SetProperty(ref _Journal, value); }
        #endregion
        public SongOverviewViewModel(ISongStore songStore, IDialogService dialog, IDurationResolver durationResolver)
        {
            SongStore = songStore;
            DialogService = dialog;
            DurationResolver = durationResolver;
            SaveCommand = new DelegateCommand(Save).ObservesCanExecute(() => NewNameValid);
            DeleteSongCommand = new DelegateCommand(ShowDeletionConfirmation);
            ChangeSourceCommand = new DelegateCommand(ChangeSource);
        }

        #region Methods
        private void ChangeSource()
        {
            if (string.IsNullOrEmpty(YoutubeID))
            {
                OpenFileDialog ofd = new OpenFileDialog()
                {
                    AddExtension = true,
                    CheckFileExists = true,
                    Filter = "Music file|*.mp3",
                    InitialDirectory = Path.GetDirectoryName(Source),
                    FileName = Path.GetFileName(Source),
                    Multiselect = false,
                };

                if (ofd.ShowDialog(App.Current.MainWindow) == true)
                {
                    if (ofd.FileName != Song.Path.LocalPath)
                    {
                        Mouse.OverrideCursor = Cursors.Wait;

                        _SourceChanged = true;
                        Source = ofd.FileName;
                        _NewDuration = DurationResolver.Resolve(Song.Path);
                        Duration = _NewDuration.FormatNice();

                        Mouse.OverrideCursor = null;
                    }
                    else
                    {
                        Source = Song.Path.LocalPath;
                        Duration = Song.Duration.FormatNice();
                        _SourceChanged = false;
                    }
                }
            }
        }
        private void ShowDeletionConfirmation() => DialogService.ConfirmDeletion(DeleteSong, Song);
        private void DeleteSong(IDialogResult result)
        {
            if (result.Result == ButtonResult.Yes)
            {
                SongStore.Remove(Song);
                GoBackCommand.Execute();
            }
        }
        private void NewNameChanged()
        {
            string trimmed = NewName.Trim();
            NewNameValid = (NewName.Length == 0 || trimmed.Equals(CurrentName) || !SongStore.ContainsByName(trimmed));
        }
        private void Save()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            if (_SourceChanged)
            {
                Song.Path = new Uri(Source);
                Song.UpdateDuration(_NewDuration);

            }

            string trimmed = NewName.Trim();
            if (trimmed.Length > 0 && !trimmed.Equals(CurrentName))
            {
                Song.Title = trimmed;
                Song.Save();
            }
            Mouse.OverrideCursor = null;

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
