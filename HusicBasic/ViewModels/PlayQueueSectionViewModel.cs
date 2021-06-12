using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using HusicBasic.Events;
using HusicBasic.Models;
using HusicBasic.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace HusicBasic.ViewModels
{
    public class PlayQueueSectionViewModel : BindableBase
    {
        #region Properties
        public string Name { get; private set; }
        public ObservableCollection<SongModel> Songs { get; private set; }
        public ObservableCollection<QueueSongViewModel> SongViewModels { get; private set; }
        public string NameArrow { get; private set; }
        public int NameIndex { get; private set; }
        public int ListIndex { get; private set; }
        public Visibility Visible { get; private set; }
        public Thickness NameMargin { get; private set; }
        public Thickness ClearMargin { get; private set; }
        public Thickness BorderMargin { get; private set; }
        public Thickness BorderPadding { get; private set; }
        public QueueSource Source { get; private set; }
        public IPlayQueue Queue { get; private set; }
        public DelegateCommand ClearSection { get; private set; }
        #endregion
        public PlayQueueSectionViewModel(string name, ObservableCollection<SongModel> songs, bool nameToTop, QueueSource source, IPlayQueue queue)
        {
            Name = name;
            Songs = songs;
            Source = source;
            Queue = queue;

            SongViewModels = new ObservableCollection<QueueSongViewModel>();
            for (int i = 0; i < songs.Count; i++)
                SongViewModels.Add(CreateSong(songs[i], i));

            if (nameToTop)
            {
                NameIndex = 1;
                ListIndex = 0;
                NameArrow = App.Current.FindResource("Text.Arrow.Up") as string;
                NameMargin = new Thickness(0, -5, 0, 2);
                ClearMargin = new Thickness(5, -3, 0, 2);
                BorderMargin = new Thickness(5.5, 5, 5.5, -4.5);
                BorderPadding = new Thickness(5, 5, 5, 10);
            }
            else
            {
                NameIndex = 0;
                ListIndex = 1;
                NameArrow = App.Current.FindResource("Text.Arrow.Down") as string;
                NameMargin = new Thickness(0, 2, 0, -5);
                ClearMargin = new Thickness(5, 2, 0, -7);
                BorderMargin = new Thickness(5.5, -4.5, 5.5, 5);
                BorderPadding = new Thickness(5, 10, 5, 5);
            }

            songs.CollectionChanged += Songs_CollectionChanged;
            Songs_CollectionChanged(null, null);

            ClearSection = new DelegateCommand(() => queue.Clear(Source));
        }

        #region events
        private QueueSongViewModel CreateSong(SongModel song, int index) => new QueueSongViewModel(song, Source, index, Queue);
        private void Songs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e != null)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        SongModel song = e.NewItems[i] as SongModel;
                        int index = e.NewStartingIndex + i;
                        SongViewModels.Insert(index, CreateSong(song, index));
                    }
                    for (int i = e.NewStartingIndex + e.NewItems.Count; i < SongViewModels.Count; i++)
                        SongViewModels[i].Index = i;
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    for (int i = 0; i < e.OldItems.Count; i++)
                        SongViewModels.RemoveAt(e.OldStartingIndex);
                    for (int i = e.OldStartingIndex; i < SongViewModels.Count; i++)
                        SongViewModels[i].Index = i;
                }
                else if (e.Action == NotifyCollectionChangedAction.Reset)
                    SongViewModels.Clear();
                else if (e.Action == NotifyCollectionChangedAction.Replace)
                {
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        int index = e.NewStartingIndex + i;
                        SongViewModels[index] = CreateSong(Songs[index], index);
                    }
                }
            }

            Visible = Songs.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            RaisePropertyChanged(nameof(Visible));
        }
        #endregion
    }
}
