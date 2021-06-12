using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using DryIoc;
using HusicBasic.Models;
using HusicBasic.Models.Tasks;
using HusicBasic.Services;
using HusicBasic.Services.Interfaces;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace HusicBasic.ViewModels.Dialogs.AddSong
{
    public class AddSongMethodYoutubeViewModel : BindableBase, INavigationAware
    {
        #region Private
        private bool _IsUrlValid;
        private bool _CanChangeUrl = true;
        private string _UrlValidText;
        private bool _ShowDetails;
        private string _SongUrl;
        private readonly IYoutubeInfoProvider InfoProvider;
        private readonly ISongStore SongStore;
        private readonly IContainerProvider Container;
        private readonly ITaskManager TaskManager;
        private Dispatcher Dispatcher;
        private Uri _Thumbnail;
        private string _CustomName;
        private string _OriginalName;
        private string _Views;
        private string _Duration;
        private string _RatingStars;
        private string _UploadDate;
        private string _Channel;
        private string _ChannelUrl;
        private DelegateCommand _AddSong;
        private DelegateCommand<IDialogParameters> _Close;
        private string _AddError;
        private YoutubeSongInfo SongInfo;
        #endregion

        #region Properties
        public DelegateCommand<IDialogParameters> Close { get => _Close; private set => SetProperty(ref _Close, value); }
        public bool IsUrlValid { get => _IsUrlValid; private set => SetProperty(ref _IsUrlValid, value); }
        public bool CanChangeUrl { get => _CanChangeUrl; private set => SetProperty(ref _CanChangeUrl, value); }
        public string UrlValidText { get => _UrlValidText; private set => SetProperty(ref _UrlValidText, value); }
        public bool ShowDetails { get => _ShowDetails; private set => SetProperty(ref _ShowDetails, value); }
        public string SongUrl { get => _SongUrl; set => SetProperty(ref _SongUrl, value, SongUrlChanged); }
        public Uri Thumbnail { get => _Thumbnail; private set => SetProperty(ref _Thumbnail, value); }
        public string CustomName{ get => _CustomName; set => SetProperty(ref _CustomName, value); }
        public string OriginalName { get => _OriginalName; private set => SetProperty(ref _OriginalName, value); }
        public string Views { get => _Views; private set => SetProperty(ref _Views, value); }
        public string Duration { get => _Duration; private set => SetProperty(ref _Duration, value); }
        public string RatingStars { get => _RatingStars; private set => SetProperty(ref _RatingStars, value); }
        public string UploadDate { get => _UploadDate; private set => SetProperty(ref _UploadDate, value); }
        public string Channel { get => _Channel; private set => SetProperty(ref _Channel, value); }
        public string ChannelUrl { get => _ChannelUrl; private set => SetProperty(ref _ChannelUrl, value); }
        public DelegateCommand AddSong { get => _AddSong; private set => SetProperty(ref _AddSong, value); }
        public string AddError { get => _AddError; private set => SetProperty(ref _AddError, value); }
        #endregion
        public AddSongMethodYoutubeViewModel(IContainerProvider container)
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
            InfoProvider = container.Resolve<IYoutubeInfoProvider>();
            SongStore = container.Resolve<ISongStore>();
            Container = container;
            TaskManager = container.Resolve<ITaskManager>();
            AddSong = new DelegateCommand(AddSongCallback, CanExecuteAddSong).ObservesProperty(() => CustomName).ObservesProperty(() => OriginalName).ObservesProperty(() => IsUrlValid);
        }

        #region Methods
        private bool CanExecuteAddSong()
        {
            if (!IsUrlValid) return false;
            string name = string.IsNullOrWhiteSpace(CustomName) ? OriginalName : CustomName;
            if (SongStore.ContainsByYoutubeID(SongInfo.ID))
            {
                AddError = "This youtube song has already been added.";
                return false;
            }
            else if (SongStore.ContainsByName(name))
            {
                AddError = "A song with this name has already been added.";
                return false;
            }

            AddError = "";
            return true;
        }
        private void AddSongCallback()
        {
            DialogParameters res = new DialogParameters
            {
                { "song-name", string.IsNullOrWhiteSpace(CustomName) ? OriginalName : CustomName },
                { "song-duration", SongInfo.Duration },
                { "song-id", SongInfo.ID },
                { "song-url", SongUrl }
            };
            Close.Execute(res);
        }
        private bool ValidUrl(string url)
        {
            try
            {
                new Uri(url);
                return true;
            }
            catch { return false; }
        }
        private void SongUrlChanged()
        {
            string s = SongUrl.Trim();
            if (s.Length > 0)
            {
                if (ValidUrl(s))
                {
                    CanChangeUrl = false;
                    Mouse.OverrideCursor = Cursors.Wait;
                    Task.Run(GetVideoDetails);

                }
                else
                {
                    UrlValidText = "Invalid";
                    ShowDetails = false;
                }
            }
            else
            {
                UrlValidText = "";
                ShowDetails = false;
            }
        }
        private void GetVideoDetails()
        {
            YoutubeSongInfo info = null;
            try
            {
                info = InfoProvider.AboutSong(SongUrl);
            }
            catch { }

            Dispatcher.Invoke(() => Mouse.OverrideCursor = null);

            if (info == null)
            {
                IsUrlValid = false;
                UrlValidText = "Invalid";
                ShowDetails = false;
                CanChangeUrl = true;
                return;
            }
            SongInfo = info;
            Dispatcher.Invoke(() => {

                IsUrlValid = true;
                Thumbnail = new Uri(info.Thumbnail);
                OriginalName = info.Name;
                CustomName = info.Name;
                SetRating(info.AverageRating);
                SetViews(info.Views);
                UploadDate = info.UploadDate.FormatNice();
                Duration = info.Duration.FormatNice();
                Channel = info.Channel;
                ChannelUrl = info.ChannelUrl;

                UrlValidText = "Ok";
                IsUrlValid = true;
                CanChangeUrl = true;
                ShowDetails = true;
            });
        }
        private void SetViews(double views)
        {
            string suffix = "";
            int d = 0;
            if (views >= 1_000_000_000)
            {
                views /= 1_000_000_000;
                suffix = "b";
            }
            else if (views >= 1_000_000)
            {
                views /= 1_000_000;
                suffix = "m";
            }
            else if (views >= 1_000)
            {
                views /= 1_000;
                suffix = "k";
            }
            if (views >= 100) d = 0;
            else if (views >= 10) d = 1;
            else if (views >= 1) d = 2;

            Views = $"{views.ToString($"N{d}")}{suffix}";
            
        }
        private void SetRating(double rating)
        {
            RatingStars = "";
            string full = App.Current.FindResource("Text.Star.Full") as string;
            string half = App.Current.FindResource("Text.Star.Left") as string;
            while (rating >= 1)
            {
                RatingStars += full;
                rating -= 1;
            }
            while (rating >= 0.5)
            {
                RatingStars += half;
                rating -= 0.5;
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.TryGetValue("close-command", out DelegateCommand<IDialogParameters> close))
                Close = close;
        }
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
        #endregion
    }
}
