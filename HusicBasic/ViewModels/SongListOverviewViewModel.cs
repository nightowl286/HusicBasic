using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using HusicBasic.Models;
using HusicBasic.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using HusicBasic;
using Prism.Services.Dialogs;
using Prism.Events;
using HusicBasic.Events;
using HusicBasic.Views;
using Prism.Ioc;
using HusicBasic.Models.Tasks;
using HusicBasic.Services.Interfaces;

namespace HusicBasic.ViewModels
{
    public class SongListOverviewViewModel : BindableBase, INavigationAware
    {
        #region Private
        private ObservableCollection<SongModel> _Results;
        private ObservableCollection<SongViewViewModel> _Songs;
        private readonly ISongStore SongStore;
        private readonly IHusicPlayer Player;
        private readonly IDialogService DialogService;
        private readonly IRegionManager RegionManager;
        private readonly IContainerProvider Container;
        private string _Query = "";
        private DelegateCommand _ShowAddSongPopup;
        private readonly SongRemovedEvent SongRemoved;
        private readonly DelegateCommand<SongModel> ShowSongOverviewCommand;
        #endregion

        #region Properties
        public DelegateCommand ShowAddSongPopup { get => _ShowAddSongPopup; private set => SetProperty(ref _ShowAddSongPopup, value); }
        public ObservableCollection<SongViewViewModel> Songs { get => _Songs; private set => SetProperty(ref _Songs, value); }
        public string Query { get => _Query; set => SetProperty(ref _Query, value, GetMatchingQuery); }

        #endregion
        public SongListOverviewViewModel(ISongStore songStore, IContainerProvider container, IHusicPlayer player, IDialogService dialogService, IEventAggregator events, IRegionManager regionManager)
        {
            ShowAddSongPopup = new DelegateCommand(() => dialogService.Show("AddSong", AddSongCallback, "Popup"));
            Container = container;
            SongStore = songStore;
            Player = player;
            DialogService = dialogService;
            RegionManager = regionManager;
            ShowSongOverviewCommand = new DelegateCommand<SongModel>(ShowSongOverview);
            SongRemoved = events.GetEvent<SongRemovedEvent>();
            SongRemoved.Subscribe(SongRemovedCallback, s => _Results.Contains(s));

            songStore.PropertyChanged += SongStore_PropertyChanged;

            _Results = new ObservableCollection<SongModel>();
            Songs = new ObservableCollection<SongViewViewModel>();
            GetMatchingQuery();
        }

        #region Events
        private void ShowSongOverview(SongModel song)
        {
            RegionManager.RequestNavigate("MainRegion", $"{nameof(SongOverview)}",
                c => {
                    Debug.WriteLine($"Showing overview for '{song.Title}' | Success: {c.Result}");
                    if (c.Result != true)
                        Debug.WriteLine($"Error: {c.Error.Message}");
                },
                new NavigationParameters()
                {
                    {"song", song },
                });
        }
        private void SongRemovedCallback(SongModel song)
        {
            for(int i = _Results.Count-1;i>=0;i--)
            {
                if (_Results[i] == song)
                {
                    _Results.RemoveAt(i);
                    Songs.RemoveAt(i);
                }
            }
        }
        private void SongStore_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Songs")
                GetMatchingQuery();
        }
        #endregion

        #region Methods
        private void AddSongCallback(IDialogResult result)
        {
            ITask task = Container.Resolve<DownloadSongTask>();
            task.Parameters = new TaskParameters()
            {
                {"name",  result.Parameters.GetValue<string>("song-name")},
                {"duration",  result.Parameters.GetValue<TimeSpan>("song-duration")},
                {"youtube-id", result.Parameters.GetValue<string>("song-id")},
                {"url", result.Parameters.GetValue<string>("song-url")},
            };
            Container.Resolve<ITaskManager>().Add(task);
        }
        private void UpdateSongsFromResults()
        {
            for(int i = 0; i < _Results.Count;i++)
            {
                if (i < Songs.Count)
                    Songs[i].Song = _Results[i];
                else
                    Songs.Add(new SongViewViewModel(_Results[i], Player, ShowSongOverviewCommand));
            }
            while (Songs.Count > _Results.Count)
                Songs.RemoveAt(Songs.Count - 1);
        }
        private IEnumerable<SongModel> GetFromQuery()
        {
            string[] queryParts = Query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return SongStore.GetAll(song => {
                foreach (string part in queryParts)
                    if (!song.Title.Contains(part, StringComparison.OrdinalIgnoreCase))
                        return false;
                return true;
            });
        }
        private void GetMatchingQuery()
        {
            IEnumerable<SongModel> newSongs = string.IsNullOrWhiteSpace(Query) ? SongStore.GetAll() : GetFromQuery();
            foreach(SongModel song in newSongs)
            {
                if (!_Results.Contains(song))
                    _Results.Add(song);
            }

            for(int i = Songs.Count-1; i>= 0; i--)
            {
                SongModel song = _Results[i];
                if (!newSongs.Contains(song))
                    _Results.RemoveAt(i);
            }
            _Results.Sort();
            UpdateSongsFromResults();
        }
        public void OnNavigatedTo(NavigationContext navigationContext) { }
        public bool IsNavigationTarget(NavigationContext navigationContext) => true;
        public void OnNavigatedFrom(NavigationContext navigationContext) { }
        #endregion


    }
}
