using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using HusicBasic.Events;
using HusicBasic.Models;
using HusicBasic.Services;
using HusicBasic.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace HusicBasic.ViewModels
{
    public class PlaylistListOverviewViewModel : BindableBase, INavigationAware
    {
        #region Private
        private ObservableCollection<PlaylistModel> _Results;
        private ObservableCollection<PlaylistViewViewModel> _Playlists;
        private readonly IPlaylistStore PlaylistStore;
        private readonly IHusicPlayer Player;
        private readonly IDialogService DialogService;
        private readonly IRegionManager RegionManager;
        private string _Query = "";
        private readonly PlaylistRemovedEvent PlaylistRemoved;
        private readonly DelegateCommand<PlaylistModel> ShowPlaylistOverviewCommand;
        #endregion

        #region Properties
        public ObservableCollection<PlaylistViewViewModel> Playlists { get => _Playlists; private set => SetProperty(ref _Playlists, value); }
        public string Query { get => _Query; set => SetProperty(ref _Query, value, GetMatchingQuery); }

        #endregion
        public PlaylistListOverviewViewModel(IPlaylistStore playlistStore, IHusicPlayer player, IDialogService dialogService, IEventAggregator events, IRegionManager regionManager)
        {
            PlaylistStore = playlistStore;
            Player = player;
            DialogService = dialogService;
            RegionManager = regionManager;
            ShowPlaylistOverviewCommand = new DelegateCommand<PlaylistModel>(ShowPlaylistOverview);
            PlaylistRemoved = events.GetEvent<PlaylistRemovedEvent>();
            PlaylistRemoved.Subscribe(PlaylistRemovedCallback, s => _Results.Contains(s));

            PlaylistStore.PropertyChanged += PlaylistStore_PropertyChanged;

            _Results = new ObservableCollection<PlaylistModel>();
            Playlists = new ObservableCollection<PlaylistViewViewModel>();
            GetMatchingQuery();
        }

        #region Events
        private void ShowPlaylistOverview(PlaylistModel Playlist)
        {
            RegionManager.RequestNavigate("MainRegion", $"{nameof(PlaylistOverview)}",
                c => {
                    Debug.WriteLine($"Showing overview for '{Playlist.Title}' | Success: {c.Result}");
                    if (c.Result != true)
                        Debug.WriteLine($"Error: {c.Error.Message}");
                },
                new NavigationParameters()
                {
                    {"playlist", Playlist },
                });
        }
        private void PlaylistRemovedCallback(PlaylistModel Playlist)
        {
            for (int i = _Results.Count - 1; i >= 0; i--)
            {
                if (_Results[i] == Playlist)
                {
                    _Results.RemoveAt(i);
                    Playlists.RemoveAt(i);
                }
            }
        }
        private void PlaylistStore_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Playlists")
                GetMatchingQuery();
        }
        #endregion

        #region Methods
        private void UpdatePlaylistsFromResults()
        {
            for (int i = 0; i < _Results.Count; i++)
            {
                if (i < Playlists.Count)
                    Playlists[i].Playlist = _Results[i];
                else
                    Playlists.Add(new PlaylistViewViewModel(_Results[i], Player, ShowPlaylistOverviewCommand));
            }
            while (Playlists.Count > _Results.Count)
                Playlists.RemoveAt(Playlists.Count - 1);
        }
        private IEnumerable<PlaylistModel> GetFromQuery()
        {
            string[] queryParts = Query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return PlaylistStore.GetAll(Playlist => {
                foreach (string part in queryParts)
                    if (!Playlist.Title.Contains(part, StringComparison.OrdinalIgnoreCase))
                        return false;
                return true;
            });
        }
        private void GetMatchingQuery()
        {
            IEnumerable<PlaylistModel> newPlaylists = string.IsNullOrWhiteSpace(Query) ? PlaylistStore.GetAll() : GetFromQuery();
            foreach (PlaylistModel Playlist in newPlaylists)
            {
                if (!_Results.Contains(Playlist))
                    _Results.Add(Playlist);
            }

            for (int i = Playlists.Count - 1; i >= 0; i--)
            {
                PlaylistModel Playlist = _Results[i];
                if (!newPlaylists.Contains(Playlist))
                    _Results.RemoveAt(i);
            }
            _Results.Sort();
            UpdatePlaylistsFromResults();
        }
        public void OnNavigatedTo(NavigationContext navigationContext) { }
        public bool IsNavigationTarget(NavigationContext navigationContext) => true;
        public void OnNavigatedFrom(NavigationContext navigationContext) { }
        #endregion
    }
}
