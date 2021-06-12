using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using HusicBasic.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using HusicBasic.Models;
using Prism.Services.Dialogs;
using System.ComponentModel;

namespace HusicBasic.ViewModels
{
    public class PopupDialogHostViewModel : BindableBase
    {
        #region Private
        private IDialogAware _DataContext;
        private object _Content;
        private Window _Owner;
        private Style _Style;
        private IDialogResult _Result;
        private Visibility _Visible = Visibility.Collapsed;
        private DelegateCommand _RequestClose;
        private PopupDialogEvent Event;
        private PopupDialog _PayloadDialog;
        private DelegateCommand<RoutedEventArgs> _LoadedCommand;
        private DelegateCommand<EventArgs> _ClosedCommand;
        private DelegateCommand<CancelEventArgs> _ClosingCommand;
        #endregion

        #region Properties
        public IDialogAware DataContext { get => _DataContext; set => SetProperty(ref _DataContext, value); }
        public object Content { get => _Content; set => SetProperty(ref _Content, value); }
        public Visibility Visible { get => _Visible; set => SetProperty(ref _Visible, value); }
        public Window Owner { get => _Owner; 
            set => SetProperty(ref _Owner, value, OwnerChanged); }
        public Style Style { get => _Style; set => SetProperty(ref _Style, value); }
        public IDialogResult Result { get => _Result; set => SetProperty(ref _Result, value); }
        public DelegateCommand RequestClose { get => _RequestClose; set => SetProperty(ref _RequestClose, value); }
        public PopupDialog PayloadDialog { get => _PayloadDialog; set => SetProperty(ref _PayloadDialog, value, PayloadDialogChanged); }
        public DelegateCommand<RoutedEventArgs> LoadedCommand { get => _LoadedCommand; set => SetProperty(ref _LoadedCommand, value); }
        public DelegateCommand<EventArgs> ClosedCommand { get => _ClosedCommand; set => SetProperty(ref _ClosedCommand, value); }
        public DelegateCommand<CancelEventArgs> ClosingCommand { get => _ClosingCommand; set => SetProperty(ref _ClosingCommand, value); }
        #endregion
        public PopupDialogHostViewModel(IEventAggregator events)
        {
            Event = events.GetEvent<PopupDialogEvent>();
            Event.Subscribe(EventReceived);
        }

        #region Events
        private void PayloadDialogChanged()
        {
            PayloadDialog.ShowRequested += Show;
            PayloadDialog.CloseRequested += Close;
            LoadedCommand = new DelegateCommand<RoutedEventArgs>(PayloadDialog.RaiseLoadedEvent);
            ClosedCommand = new DelegateCommand<EventArgs>(PayloadDialog.RaiseClosedEvent);
            ClosingCommand = new DelegateCommand<CancelEventArgs>(PayloadDialog.RaiseClosingEvent);
        }
        private void CloseIfAllowed()
        {
            if (DataContext != null)
            {
                if (DataContext.CanCloseDialog())
                    Close();
            }
        }
        private void EventReceived((PopupDialog,string) payload)
        {
            (PopupDialog dialog, string property) = payload;
            PayloadDialog = dialog;

            if (property == nameof(PopupDialog.DataContext))
            {
                if (DataContext != null)
                    DataContext.RequestClose -= DataContext_RequestClose;

                DataContext = dialog.DataContext as IDialogAware;
                if (DataContext != null)
                {
                    DataContext.RequestClose += DataContext_RequestClose;
                    RequestClose = new DelegateCommand(CloseIfAllowed);
                }
            }
            else if (property == nameof(PopupDialog.Content)) Content = dialog.Content;
            else if (property == nameof(PopupDialog.Owner)) Owner = dialog.Owner;
            else if (property == nameof(PopupDialog.Style)) Style = dialog.Style;
            else if (property == nameof(PopupDialog.Result)) Result = dialog.Result;
        }
        private void DataContext_RequestClose(IDialogResult result)
        {
            if (PayloadDialog != null) PayloadDialog.Result = result;
            Close();
        }
        private void Owner_Closed(object sender, EventArgs e)
        {
            if (Owner != null)
            {
                Owner.Closed -= Owner_Closed;
                Close();
            }
        }
        #endregion

        #region Methods
        private void OwnerChanged()
        {
            if (Owner != null)
                Owner.Closed += Owner_Closed;
        }
        public void Show()
        {
            Visible = Visibility.Visible;
        }
        public void Close()
        {
            Visible = Visibility.Collapsed;
            if (DataContext != null) DataContext.RequestClose -= DataContext_RequestClose;

            PayloadDialog?.RaiseClosedEvent(EventArgs.Empty);

            DataContext = null;
            Content = null;
        }
        #endregion

    }
}
