using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace HusicBasic.ViewModels.Dialogs.AddSong
{
    public class AddSongMethodViewViewModel : BindableBase, INavigationAware
    {

        #region Properties
        public DelegateCommand<string> ChooseMethod { get; set; }
        private DelegateCommand<IDialogParameters> CloseCommand;
        #endregion
        public AddSongMethodViewViewModel(IRegionManager manager)
        {

            ChooseMethod = new DelegateCommand<string>(s => manager.RequestNavigate("AddSongMainRegion", s,c => {
                Debug.WriteLine($"AddSongDialog Navigation: {c.Result}");
            }, new NavigationParameters()
            {
                {"close-command",CloseCommand },
            }));
        }

        #region Methods
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.TryGetValue("close-command", out DelegateCommand<IDialogParameters> close))
                CloseCommand = close;
        }
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
        #endregion
    }
}
