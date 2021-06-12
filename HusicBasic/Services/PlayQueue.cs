using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using HusicBasic.Events;
using HusicBasic.Models;
using HusicBasic.ViewModels;
using Prism.Events;
using Prism.Mvvm;

namespace HusicBasic.Services
{
    public class PlayQueue : BindableBase, IPlayQueue
    {
        #region Private
        private ObservableCollection<SongModel> _History;
        private ObservableCollection<SongModel> _Next;
        private ObservableCollection<SongModel> _Queue;
        private ObservableCollection<SongModel> _Repeating;
        private SongModel _Current;
        private bool _CanGetPrevious;
        private bool _CanGetNext;
        private int _Position;
        private ISongShuffler _Shuffler;
        private IHusicPlayer _Player;
        private int _MaxHistorySize = 20;
        public event Action<IPlayQueue, SongModel> SongRequested;
        private TimeSpan _UpcomingDuration;
        private readonly SongRemovedEvent SongRemoved;
        #endregion

        #region Properties
        public ObservableCollection<SongModel> History { get => _History; private set => SetProperty(ref _History, value); }
        public ObservableCollection<SongModel> Next { get => _Next; private set => SetProperty(ref _Next, value); }
        public ObservableCollection<SongModel> Queue { get => _Queue; private set => SetProperty(ref _Queue, value); }
        public ObservableCollection<SongModel> Repeating { get => _Repeating; private set => SetProperty(ref _Repeating, value); }
        public bool CanGetPrevious { get => _CanGetPrevious; private set => SetProperty(ref _CanGetPrevious, value); }
        public bool CanGetNext { get => _CanGetNext; private set => SetProperty(ref _CanGetNext, value); }
        public int Position { get => _Position; private set => SetProperty(ref _Position, value, PositionChanged); }
        public SongModel Current { get => _Current; private set => SetProperty(ref _Current, value); }
        public ISongShuffler Shuffler { get => _Shuffler; set => SetProperty(ref _Shuffler, value); }
        public bool PlayingFromHistory => Position != 0;
        public int MaxHistorySize { get => _MaxHistorySize; set => SetProperty(ref _MaxHistorySize, value, EnsureHistorySize); }
        public TimeSpan UpcomingDuration { get => _UpcomingDuration; private set => SetProperty(ref _UpcomingDuration, value); }
        public IHusicPlayer Player { get => _Player; set => SetProperty(ref _Player, value); }
        #endregion
        public PlayQueue(IEventAggregator events)
        {
            SongRemoved = events.GetEvent<SongRemovedEvent>();
            SongRemoved.Subscribe(RemoveSong);

            History = new ObservableCollection<SongModel>();
            Next = new ObservableCollection<SongModel>();
            Queue = new ObservableCollection<SongModel>();
            Repeating = new ObservableCollection<SongModel>();

            History.CollectionChanged += History_CollectionChanged;
            Next.CollectionChanged += Next_CollectionChanged;
            Queue.CollectionChanged += Queue_CollectionChanged;
            Repeating.CollectionChanged += Repeating_CollectionChanged;


            Current = null;
            Position = 0;
        }

        #region Events

        private void History_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => UpdateUpcomingDuration();
        private void Next_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => UpdateUpcomingDuration();
        private void Queue_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => UpdateUpcomingDuration();
        private void Repeating_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => UpdateUpcomingDuration();

        #endregion

        #region Methods
        public void RemoveSong(SongModel song)
        {
            RemoveSongFromCollection(History, song);
            RemoveSongFromCollection(Next, song);
            RemoveSongFromCollection(Queue, song);
            RemoveSongFromCollection(Repeating, song);
            if (Current == song)
                Current = null;

            UpdateCanGetPrevious();
            UpdateCanGetNext();
        }
        private void RemoveSongFromCollection(ObservableCollection<SongModel> collection, SongModel song)
        {
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (collection[i] == song)
                    collection.RemoveAt(i);
            }
        }
        private void PositionChanged()
        {
            RaisePropertyChanged(nameof(PlayingFromHistory));
            UpdateCanGetNext();
            UpdateCanGetPrevious();
        }
        private void UpdateCanGetNext()
        {
            if (Position == 0 || (Position == -1 && Current == null))
                CanGetNext = Next.Count > 0 || Queue.Count > 0;
            else
                CanGetNext = true;
        }
        private void UpdateCanGetPrevious() => CanGetPrevious = Math.Abs(Position) < History.Count;
        public SongModel GetNext()
        {
            SongModel song;
            if (Position == 0 || ((Position == -1 || Math.Abs(Position) > History.Count) && Current == null))
            {
                if (Next.Count > 0)
                {
                    song = Next[0];
                    Next.RemoveAt(0);
                }
                else
                {
                    song = Queue[0];
                    Queue.RemoveAt(0);
                }

                Current = song;
                Position = 0;

                UpdateCanGetNext();
                UpdateCanGetPrevious();
            }
            else if ((Position == -1 || Math.Abs(Position) > History.Count) && Current != null)
            {
                song = Current;
                Position = 0;
            }
            else
                song = History[History.Count + (++Position)];


            return song;
        }
        public SongModel GetPrevious()
        {
            return History[History.Count + (--Position)];
        }
        public void ReshuffleQueue()
        {
            if (Queue.Count == 0 && Repeating.Count > 0)
                AppendRepeating();
            else
            {
                IList<SongModel> shuffled = Shuffler.Shuffle(Queue);
                for (int i = 0; i < shuffled.Count; i++)
                {
                    if (i < Queue.Count)
                        Queue[i] = shuffled[i];
                    else
                        Queue.Add(shuffled[i]);
                }
                while (Queue.Count > shuffled.Count)
                    Queue.RemoveAt(Queue.Count - 1);

                UpdateCanGetPrevious();
                UpdateCanGetNext();
            }

        }
        public void AppendRepeating()
        {
            IList<SongModel> shuffled = Shuffler.Shuffle(Repeating);
            Queue.AddRange(shuffled);
            UpdateCanGetNext();
            UpdateCanGetPrevious();
        }
        public void SetCurrent(SongModel current)
        {
            Current = current;
            Position = 0;
        }
        public void MoveCurrentToHistory()
        {
            if (Current != null)
            {
                History.Add(Current);
                Current = null;
                EnsureHistorySize();
                UpdateCanGetPrevious();
            }
        }
        public void SetRepeating(IEnumerable<SongModel> songs)
        {
            Queue.Clear();
            Repeating = new ObservableCollection<SongModel>(songs);
            AppendRepeating();
            UpdateCanGetNext();
        }
        public void AddToRepeating(IEnumerable<SongModel> songs)
        {
            Repeating.AddRange(songs);
            Queue.AddRange(songs);
            ReshuffleQueue();
            UpdateCanGetNext();
        }
        public void AddToRepeating(SongModel song)
        {
            Repeating.Add(song);
            Queue.Add(song);
            ReshuffleQueue();
            UpdateCanGetNext();
        }
        public void InsertInNext(SongModel song)
        {
            Next.Insert(0,song);
            UpdateCanGetNext();
        }
        public void InsertInNext(IEnumerable<SongModel> songs)
        {
            int i = 0;
            foreach(SongModel song in songs)
                Next.Insert(i++, song);
            UpdateCanGetNext();
        }
        public void EnsureHistorySize()
        {
            if (History.Count > MaxHistorySize)
            {
                while (History.Count > MaxHistorySize)
                    History.RemoveAt(0);
            }
        }
        public void PlayFrom(QueueSource source, int index)
        {
            SongModel song = null;
            if (source == QueueSource.Current)
            {
                song = Current;
                Position = 0;
            }
            else if (source == QueueSource.History)
            {
                song = History[index];
                Position = index - History.Count;
            }
            else if (source == QueueSource.Next)
            {
                Position = 0;
                for (int i = 0; i < index + 1; i++)
                {
                    MoveCurrentToHistory();
                    song = GetNext();
                }
            }
            else if (source == QueueSource.Queue)
            {
                Position = 0;
                for (int i = 0; i < index + Next.Count + 1; i++)
                {
                    MoveCurrentToHistory();
                    song = GetNext();
                }
            }

            if (song != null)
                SongRequested?.Invoke(this, song);
        }
        public void RemoveFrom(QueueSource source, int index)
        {
            if (source == QueueSource.History)
                History.RemoveAt(index);
            else if (source == QueueSource.Next)
            {
                Next.RemoveAt(index);
                AppendRepeatingIfNeeded();
            }
            else if (source == QueueSource.Queue)
            {
                Queue.RemoveAt(index);
                AppendRepeatingIfNeeded();
            }
            else if (source == QueueSource.Current)
            {
                Current = null;
                if (Next.Count > 0)
                {
                    Current = Next[0];
                    Next.RemoveAt(0);
                }
                else if (Queue.Count > 0)
                {
                    Current = Queue[0];
                    Queue.RemoveAt(0);

                    AppendRepeatingIfNeeded();
                }
                else if (Player?.Repeat == RepeatMode.All && Repeating.Count > 0)
                {
                    AppendRepeating();
                    Current = Queue[0];
                    Queue.RemoveAt(0);

                    if (Queue.Count == 0) AppendRepeating();
                }
            }

            UpdateCanGetPrevious();
            UpdateUpcomingDuration();
            UpdateCanGetNext();
        }
        private void AppendRepeatingIfNeeded()
        {
            if (Queue.Count == 0 && Player?.Repeat == RepeatMode.All && Repeating.Count > 0)
                AppendRepeating();
        }
        public void Clear(QueueSource source)
        {
            if (source == QueueSource.Current) throw new ArgumentException("Cannot clear the current as it is not a seperate queue.", nameof(source));
            if (source == QueueSource.History)
            {
                SongModel song = PlayingFromHistory ? History[Math.Abs(Position) - 1] : null;
                History.Clear();
                if (song != null)
                {
                    History.Add(song);
                    Position = -1;
                }
            }
            else if (source == QueueSource.Next) Next.Clear();
            else if (source == QueueSource.Queue)
            {
                Queue.Clear();
                Repeating.Clear();
            }

            UpdateCanGetNext();
            UpdateCanGetPrevious();

        }
        private TimeSpan GetDuration(ObservableCollection<SongModel> songs)
        {
            TimeSpan time = TimeSpan.Zero;
            foreach (SongModel song in songs)
                time += song.Duration;
            return time;
        }
        public void UpdateUpcomingDuration() => UpcomingDuration = GetUpcomingDuration();
        public TimeSpan GetUpcomingDuration()
        {
            TimeSpan next = GetDuration(Next);
            TimeSpan queue = GetDuration(Queue);
            TimeSpan total = next + queue;
            if (PlayingFromHistory)
            {
                if (History.Count > 0)
                {
                    for (int i = Math.Abs(Position) - 2; i >= 0; i--)
                        total += History[i].Duration;
                }
                if (Current != null)
                    total += Current.Duration;
            }
            return total;
        }
        #endregion
    }
}
