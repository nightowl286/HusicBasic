using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using HusicBasic.Services;
using Prism.Mvvm;

namespace HusicBasic.Models
{
    [DebuggerDisplay("[#{ID}] {Title}")]
    public class SongModel : BindableBase, IEquatable<SongModel>, IComparable<SongModel>
    {
        #region Fields
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private uint _ID;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Uri _Path;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _Title;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TimeSpan _Duration;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ISongStore Store;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _YoutubeID;
        #endregion

        #region Properties
        public uint ID { get => _ID; private set => SetProperty(ref _ID, value); }
        public Uri Path { get => _Path; private set => SetProperty(ref _Path, value); }
        public string Title { get => _Title; set => SetProperty(ref _Title, value); }
        public TimeSpan Duration { get => _Duration; private set => SetProperty(ref _Duration, value); }
        public string YoutubeID { get => _YoutubeID; private set => SetProperty(ref _YoutubeID, value); }
        #endregion
        public SongModel(ISongStore store, uint id, Uri path, TimeSpan duration, string title, string ytID)
        {
            Store = store;
            ID = id;
            Path = path;
            Duration = duration;
            Title = title;
            YoutubeID = ytID;
        }

        #region Methods
        public void UpdateDuration(TimeSpan newDuration)
        {
            Duration = newDuration;
            Save();
        }
        public void Save() => Store.Save(this);
        public void Remove() => Store.Remove(this);
        #endregion

        #region Equal and Compare
        public bool Equals([AllowNull] SongModel other) => other == this;
        public int CompareTo([AllowNull] SongModel other) => StringComparer.CurrentCultureIgnoreCase.Compare(this?.Title, other?.Title);
        #endregion

    }
}
