using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using HusicBasic.Events;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace HusicBasic.Models
{
    public class PopupDialog : BindableBase, IDialogWindow
    {
        #region Private
        private object _Content;
        private object _DataContext;
        private Window _Owner;
        private IDialogResult _Result;
        private Style _Style;
        private readonly PopupDialogEvent Event;
        #endregion

        #region Events
        public event RoutedEventHandler Loaded;
        public event EventHandler Closed;
        public event CancelEventHandler Closing;
        public event Action CloseRequested;
        public event Action ShowRequested;
        #endregion

        #region Properties
        public object Content { get => _Content; set => SetProperty(ref _Content, value); }
        public object DataContext { get => _DataContext; set => SetProperty(ref _DataContext, value); }
        public Window Owner { get => _Owner; set => SetProperty(ref _Owner, value); }
        public IDialogResult Result { get => _Result; set => SetProperty(ref _Result, value); }
        public Style Style { get => _Style; set => SetProperty(ref _Style, value); }
        #endregion
        public PopupDialog(IEventAggregator events)
        {
            Event = events.GetEvent<PopupDialogEvent>();
            PropertyChanged += PopupDialog_PropertyChanged;
        }

        #region Events
        private void PopupDialog_PropertyChanged(object sender, PropertyChangedEventArgs e) => Event.Publish((this, e.PropertyName));
        #endregion

        #region Methods
        public void Close() => CloseRequested?.Invoke();
        public void Show() => ShowRequested?.Invoke();
        public bool? ShowDialog() => throw new NotSupportedException("Please only use 'Show' on the popup dialog.");
        public void RaiseLoadedEvent(RoutedEventArgs e) => Loaded?.Invoke(this, e);
        public void RaiseClosedEvent(EventArgs e) => Closed?.Invoke(this, e);
        public void RaiseClosingEvent(CancelEventArgs e) => Closing?.Invoke(this, e);
        #endregion
    }
}
