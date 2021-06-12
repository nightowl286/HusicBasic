using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using HusicBasic.Models;
using HusicBasic.Services;
using Prism.Commands;
using Prism.Mvvm;

namespace HusicBasic.ViewModels
{
    public class QueueSongViewModel : BindableBase
    {
        #region Private
        private SongModel _Song;
        private SongModel _LastSong;
        private IPlayQueue _Queue;
        private QueueSource _Source;
        private int _Index;
        private DelegateCommand _PlaySongCommand;
        private DelegateCommand _RemoveFromSection;
        private string _Title;
        private bool _IsPlaying;
        private Visibility _ShowRemoveButton;
        #endregion

        #region Properties
        public SongModel Song { get => _Song; private set => SetProperty(ref _Song, value, CurrentSongChanged); }
        public IPlayQueue Queue { get => _Queue; private set => SetProperty(ref _Queue, value); }
        public QueueSource Source { get => _Source; set => SetProperty(ref _Source, value); }
        public int Index { get => _Index; set => SetProperty(ref _Index, value); }
        public DelegateCommand PlaySongCommand { get => _PlaySongCommand; private set => SetProperty(ref _PlaySongCommand, value); }
        public DelegateCommand RemoveFromSection { get => _RemoveFromSection; private set => SetProperty(ref _RemoveFromSection, value); }
        public string Title { get => _Title; private set => SetProperty(ref _Title, value); }
        public bool IsPlaying { get => _IsPlaying; private set => SetProperty(ref _IsPlaying, value); }
        public Visibility ShowRemoveButton { get => _ShowRemoveButton; private set => SetProperty(ref _ShowRemoveButton, value); }
        #endregion
        public QueueSongViewModel(SongModel song, QueueSource source, int index, IPlayQueue queue)
        {
            Song = song;
            Source = source;
            Index = index;
            Queue = queue;
            PlaySongCommand = new DelegateCommand(PlaySong, () => !IsPlaying).ObservesProperty(() => IsPlaying);
            RemoveFromSection = new DelegateCommand(RemoveSong, () => !IsPlaying).ObservesProperty(() => IsPlaying);
            Title = song.Title;
            ShowRemoveButton = IsPlaying ? Visibility.Collapsed : Visibility.Visible;
            if (Source == QueueSource.Current || source == QueueSource.History)
            {
                queue.PropertyChanged += Queue_PropertyChanged;
                CheckCurrent();
            }
        }

        #region Events
        private void Queue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IPlayQueue.PlayingFromHistory) || e.PropertyName == nameof(IPlayQueue.Position))
                CheckCurrent();
        }
        private void Song_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SongModel.Title))
                Title = Song.Title;
        }
        #endregion

        #region Methods
        private void CurrentSongChanged()
        {
            if (_LastSong != null) _LastSong.PropertyChanged -= Song_PropertyChanged;
            Song.PropertyChanged += Song_PropertyChanged;
            _LastSong = Song;
        }
        private void CheckCurrent()
        {
            if (!Queue.PlayingFromHistory && Source == QueueSource.Current)
                IsPlaying = true;
            else if (Queue.PlayingFromHistory && Source == QueueSource.History)
                IsPlaying = (Queue.History.Count + Queue.Position == Index);
            else IsPlaying = false;
            ShowRemoveButton = IsPlaying ? Visibility.Collapsed : Visibility.Visible;
            PlaySongCommand.RaiseCanExecuteChanged();
        }
        private void PlaySong() => Queue.PlayFrom(Source, Index);
        private void RemoveSong() => Queue.RemoveFrom(Source, Index);
        #endregion
    }
}
