using System;
using System.Collections.Generic;
using System.Linq;
using HusicBasic.Services.Interfaces;
using HusicBasic.Views.Dialogs.AddSong;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace HusicBasic.ViewModels.Dialogs.AddSong
{
    public class AddSongDialogViewModel : BindableBase, IDialogAware
    {
        #region Private
        private object _CurrentPage;
        private bool _CanClose = true;
        private readonly IContainerProvider Container;
        private readonly IRegionManager RegionManager;
        private DelegateCommand _LoadedCommand;
        private DelegateCommand<IDialogParameters> _CloseCommand;
        private string _NavigationTarget;
        private DialogParameters _ExtraParameters;
        #endregion

        #region Properties
        public object CurrentPage { get => _CurrentPage; private set => SetProperty(ref _CurrentPage, value); }
        public string Title => "Add song";
        public event Action<IDialogResult> RequestClose;
        public DelegateCommand LoadedCommand { get => _LoadedCommand; private set => SetProperty(ref _LoadedCommand, value); }
        #endregion
        public AddSongDialogViewModel(IContainerProvider container, IRegionManager manager)
        {
            Container = container;
            _CloseCommand = new DelegateCommand<IDialogParameters>(Close);
            LoadedCommand = new DelegateCommand(Loaded);
            RegionManager = manager;

        }

        #region Methods
        private void Loaded()
        {
            RegionManager.RequestNavigate("AddSongMainRegion", "AddSongMethodView", new NavigationParameters()
            {
                {"close-command",_CloseCommand }
            });
        }
        public void Close(IDialogParameters parameters) => RequestClose?.Invoke(new DialogResult(ButtonResult.None, parameters));
        public bool CanCloseDialog() => _CanClose;
        public void OnDialogClosed() 
        {
            
        }
        public void OnDialogOpened(IDialogParameters parameters)
        {
        }
        #endregion
    }
}
