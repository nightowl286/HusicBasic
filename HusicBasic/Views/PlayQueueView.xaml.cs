using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using HusicBasic.Services;
using HusicBasic.ViewModels;

namespace HusicBasic.Views
{
    /// <summary>
    /// Interaction logic for PlayQueueView
    /// </summary>
    public partial class PlayQueueView : UserControl
    {
        #region Variables
        private DateTime? LastScroll;
        private uint IgnoreScroll = 1;
        #endregion
        public PlayQueueView(IPlayQueue queue)
        {
            InitializeComponent();
            queue.PropertyChanged += Queue_PropertyChanged;
        }

        #region Events
        private void Queue_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IPlayQueue.PlayingFromHistory) || e.PropertyName == nameof(IPlayQueue.Position) || e.PropertyName == nameof(IPlayQueue.Current))
            {
                if (LastScroll == null || DateTime.Now.Subtract(LastScroll.Value).TotalSeconds >= 5)
                {
                    IgnoreScroll++;
                    ScrollSongToView();
                    
                }
            }
        }
        #endregion

        #region Methods
        private void ScrollSongToView()
        {
            ICollection<QueueSongView> songViews = this.GetChildrenOfType<QueueSongView>();
            foreach(QueueSongView songView in songViews)
            {
                if (songView.DataContext is QueueSongViewModel vm && vm.IsPlaying)
                {
                    songView.BringIntoView();
                    return;
                }

            }
        }
        #endregion

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (IgnoreScroll > 0)
                IgnoreScroll--;
            else
                LastScroll = DateTime.Now;
        }
    }
}
