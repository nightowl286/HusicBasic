using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using HusicBasic.Events;
using HusicBasic.Services;
using Prism.Events;
using Prism.Mvvm;

namespace HusicBasic.Models
{
    [DebuggerDisplay("[#{ID}] {Title} ({Count})")]
    public class PlaylistModel : BindableBase, IEquatable<PlaylistModel>, IComparable<PlaylistModel>
    {
        #region Fields
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private uint _ID;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _Title;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ObservableCollection<SongModel> _Songs;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ObservableCollection<SongModel> _OldSongsRef = null;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IPlaylistStore Store;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TimeSpan _Duration;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int _Count;
        #endregion

        #region Properties
        public uint ID { get => _ID; private set => SetProperty(ref _ID, value); }
        public string Title { get => _Title; set => SetProperty(ref _Title, value); }
        public ObservableCollection<SongModel> Songs { get => _Songs; private set => SetProperty(ref _Songs, value, SongCollectionChanged); }
        public TimeSpan Duration { get => _Duration; private set => SetProperty(ref _Duration, value); }
        public int Count { get => _Count; private set => SetProperty(ref _Count, value); }
        #endregion
        public PlaylistModel(IPlaylistStore store,IEventAggregator events, uint id, string title, ObservableCollection<SongModel> songs)
        {
            Store = store;
            events.GetEvent<SongRemovedEvent>().Subscribe(Remove, s => Songs.Contains(s));
            
            ID = id;
            Title = title;
            Songs = songs;
        }

        #region Methods
        public void Add(SongModel song)
        {
            Songs.Add(song);
            Count++;
            Duration += song.Duration;
            Save();
        }
        public void Remove(SongModel song)
        {
            Songs.Remove(song);
            Count--;
            Duration -= song.Duration;
            Save();
        }
        private void SongCollectionChanged()
        {
            if (_OldSongsRef != null)
                _OldSongsRef.CollectionChanged -= SongCollectionModified;
            _OldSongsRef = _Songs;
            _Songs.CollectionChanged += SongCollectionModified;

            UpdateCount();
            UpdateDuration();
            Save();
        }
        private void SongCollectionModified(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset || e.Action == NotifyCollectionChangedAction.Replace)
            {
                UpdateCount();
                UpdateDuration();
                Save();
            }
        }
        public void UpdateDuration()
        {
            TimeSpan duration = TimeSpan.Zero;
            foreach (SongModel song in Songs)
                duration += song.Duration;
            Duration = duration;
        }
        public void UpdateCount() => Count = Songs.Count;
        public void Save() => Store.Save(this);
        public TimeSpan GetAvgDuration()
        {
            List<double> durations = new List<double>(Songs.Select(s => s.Duration.TotalSeconds));
            double correctedAverage = durations.GetCorrectedAverage();
            return TimeSpan.FromSeconds(correctedAverage);
        }
        #endregion

        #region Equal and Compare
        public bool Equals([AllowNull] PlaylistModel other) => other == this;
        public int CompareTo([AllowNull] PlaylistModel other) => StringComparer.CurrentCultureIgnoreCase.Compare(this?.Title, other?.Title);
        #endregion
    }
}

