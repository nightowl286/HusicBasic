using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using HusicBasic.Models;
using HusicBasic.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace HusicBasic.ViewModels
{
    public enum QueueSource
    {
        History,
        Current,
        Next,
        Queue
    }

    public class PlayQueueViewModel : BindableBase
    {
        #region Private
        private readonly IHusicPlayer _Player;
        private IPlayQueue _PlayQueue;
        private PlayQueueSectionViewModel _History;
        private PlayQueueSectionViewModel _Next;
        private PlayQueueSectionViewModel _Queue;
        private int _LeftToPlayCount;
        private DelegateCommand _ReshuffleQueue;
        private DelegateCommand _ClearUpcoming;
        private string _TimeLeft = "0s";
        #endregion

        #region Properties
        public IPlayQueue PlayQueue { get => _PlayQueue; private set => SetProperty(ref _PlayQueue, value);  }
        public PlayQueueSectionViewModel History { get => _History; private set => SetProperty(ref _History, value); }
        public QueueSongViewModel Current => PlayQueue?.Current == null ? null : new QueueSongViewModel(PlayQueue.Current, QueueSource.Current, 0, PlayQueue);
        public PlayQueueSectionViewModel Next { get => _Next; private set => SetProperty(ref _Next, value); }
        public PlayQueueSectionViewModel Queue { get => _Queue; private set => SetProperty(ref _Queue, value); }
        public Visibility CurrentVisible => Current == null ? Visibility.Collapsed : Visibility.Visible;
        public int LeftToPlayCount { get => _LeftToPlayCount; private set => SetProperty(ref _LeftToPlayCount, value); }
        public DelegateCommand ReshuffleQueue { get => _ReshuffleQueue; private set => SetProperty(ref _ReshuffleQueue, value); }
        public DelegateCommand ClearUpcoming { get => _ClearUpcoming; private set => SetProperty(ref _ClearUpcoming, value); }
        public string TimeLeft { get => _TimeLeft; private set => SetProperty(ref _TimeLeft, value); }
        #endregion
        public PlayQueueViewModel(IPlayQueue queue, IHusicPlayer player)
        {
            PlayQueue = queue;
            _Player = player;
            player.PropertyChanged += Player_PropertyChanged;
            queue.PropertyChanged += Queue_PropertyChanged;

            Queue_PropertyChanged(null, new PropertyChangedEventArgs(nameof(History)));
            Queue_PropertyChanged(null, new PropertyChangedEventArgs(nameof(Current)));
            Queue_PropertyChanged(null, new PropertyChangedEventArgs(nameof(Next)));
            Queue_PropertyChanged(null, new PropertyChangedEventArgs(nameof(Queue)));

            ReshuffleQueue = new DelegateCommand(queue.ReshuffleQueue, () => !(PlayQueue.Shuffler is LinearShuffler) && PlayQueue.Repeating.Count > 0).ObservesProperty(() => PlayQueue.Shuffler).ObservesProperty(() => PlayQueue.Repeating.Count);
            ClearUpcoming = new DelegateCommand(() => { PlayQueue.Clear(QueueSource.Next); PlayQueue.Clear(QueueSource.Queue); }, () => PlayQueue.Next.Count > 0 || PlayQueue.Queue.Count > 0).ObservesProperty(() => PlayQueue.Queue.Count).ObservesProperty(() => PlayQueue.Next.Count);

            UpdateTimeLeft();


        }



        #region Events
        private void Player_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IHusicPlayer.Position)) UpdateTimeLeft();
            else if (e.PropertyName == nameof(IHusicPlayer.Duration)) UpdateTimeLeft();
            else if (e.PropertyName == nameof(IHusicPlayer.CurrentSong))
                PlayQueue.UpdateUpcomingDuration();
        }
        private void Queue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IPlayQueue.History))
                History = new PlayQueueSectionViewModel("History", PlayQueue.History, true, QueueSource.History, PlayQueue);
            else if (e.PropertyName == nameof(IPlayQueue.Next))
                Next = new PlayQueueSectionViewModel("Next", PlayQueue.Next, false, QueueSource.Next, PlayQueue);
            else if (e.PropertyName == nameof(IPlayQueue.Queue))
                Queue = new PlayQueueSectionViewModel("Queue", PlayQueue.Queue, false, QueueSource.Queue, PlayQueue);
            else if (e.PropertyName == nameof(IPlayQueue.Current))
            {
                RaisePropertyChanged(nameof(Current));
                RaisePropertyChanged(nameof(CurrentVisible));
            }
            else if (e.PropertyName == nameof(IPlayQueue.UpcomingDuration))
                UpdateTimeLeft();
        }
        private void UpdateTimeLeft()
        {
            TimeSpan upcoming = PlayQueue.UpcomingDuration;
            upcoming += (_Player.Duration - _Player.Position);
            FormatTimeLeft(upcoming);
        }
        private void FormatTimeLeft(TimeSpan time)
        {
            TimeLeft = time.FormatNice();   
        }
        #endregion
    }
}
