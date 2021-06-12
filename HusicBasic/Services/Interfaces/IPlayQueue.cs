using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using HusicBasic.Models;
using HusicBasic.ViewModels;

namespace HusicBasic.Services
{
    public interface IPlayQueue : INotifyPropertyChanged
    {
        #region Properties
        event Action<IPlayQueue, SongModel> SongRequested;
        ObservableCollection<SongModel> History { get; }
        ObservableCollection<SongModel> Next { get; }
        ObservableCollection<SongModel> Queue { get; }
        ObservableCollection<SongModel> Repeating { get; }
        SongModel Current { get; }
        bool CanGetNext { get; }
        bool CanGetPrevious { get; }
        int Position { get; }
        bool PlayingFromHistory { get; }
        ISongShuffler Shuffler { get; set; }
        IHusicPlayer Player { get; set; }
        int MaxHistorySize { get; set; }
        public TimeSpan UpcomingDuration { get; }
        #endregion

        #region Methods
        void RemoveSong(SongModel song);
        SongModel GetPrevious();
        SongModel GetNext();
        void ReshuffleQueue();
        void AppendRepeating();
        void SetCurrent(SongModel current);
        void MoveCurrentToHistory();
        void SetRepeating(IEnumerable<SongModel> songs);
        void AddToRepeating(IEnumerable<SongModel> songs);
        void AddToRepeating(SongModel song);
        void InsertInNext(SongModel song);
        void InsertInNext(IEnumerable<SongModel> songs);
        void EnsureHistorySize();
        void RemoveFrom(QueueSource source, int index);
        void PlayFrom(QueueSource source, int index);
        void Clear(QueueSource source);
        void UpdateUpcomingDuration();
        TimeSpan GetUpcomingDuration();
        #endregion
    }
}
