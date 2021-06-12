using System;
using System.Collections.Generic;
using System.Text;
using Prism.Mvvm;

namespace HusicBasic.Models
{
    public class YoutubeSongInfo : BindableBase
    {
        #region Private
        private string _Name;
        private string _Thumbnail;
        private ulong _Views;
        private string _ID;
        private TimeSpan _Duration;
        private string _Channel;
        private string _ChannelUrl;
        private DateTime _UploadDate;
        private double _AverageRating;
        #endregion

        #region Public
        public string Name { get => _Name; set => SetProperty(ref _Name, value); }
        public string Thumbnail { get => _Thumbnail; set => SetProperty(ref _Thumbnail, value); }
        public ulong Views {get => _Views; set => SetProperty(ref _Views, value); }
        public string ID { get => _ID; set => SetProperty(ref _ID, value); }
        public TimeSpan Duration { get => _Duration; set => SetProperty(ref _Duration, value); }
        public string Channel { get => _Channel; set => SetProperty(ref _Channel, value); }
        public string ChannelUrl { get => _ChannelUrl; set => SetProperty(ref _ChannelUrl, value); }
        public DateTime UploadDate { get => _UploadDate; set => SetProperty(ref _UploadDate, value); }
        public double AverageRating { get => _AverageRating; set => SetProperty(ref _AverageRating, value); }
        #endregion

    }
}
