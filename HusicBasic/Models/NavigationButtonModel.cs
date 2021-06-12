using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Prism.Mvvm;

namespace HusicBasic.Models
{
    public class NavigationButtonModel : BindableBase
    {
        #region Private
        private string _Text;
        private string _Navigation;
        private bool _IsAtNavigation;
        #endregion

        #region Properties
        public string Text { get => _Text; set => SetProperty(ref _Text, value); }
        public string Navigation { get => _Navigation; set => SetProperty(ref _Navigation, value); }
        public bool IsAtNavigation { get => _IsAtNavigation; set => SetProperty(ref _IsAtNavigation, value, () => RaisePropertyChanged(nameof(CanNavigate))); }
        public bool CanNavigate { get => !_IsAtNavigation; }
        #endregion
    }
}
