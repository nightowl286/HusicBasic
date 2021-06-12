using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace HusicBasic.ViewModels.Dialogs
{
    public class ConfirmationDialogViewModel : BindableBase, IDialogAware
    {
        #region Private
        private string _Message;
        private DelegateCommand<string> _GotResult;
        #endregion

        #region Properties
        public string Title => "Confirmation";
        public event Action<IDialogResult> RequestClose;
        public string Message { get => _Message; private set => SetProperty(ref _Message, value); }
        public DelegateCommand<string> GotResult { get => _GotResult; private set => SetProperty(ref _GotResult, value); }
        #endregion
        public ConfirmationDialogViewModel() => GotResult = new DelegateCommand<string>(ResultGotten);

        #region Methods
        private void ResultGotten(string res)
        {
            ButtonResult result = ButtonResult.None;
            if (res.ToLower() == "true")
                result = ButtonResult.Yes;
            else if (res.ToLower() == "false")
                result = ButtonResult.No;

            RequestClose?.Invoke(new DialogResult(result));
        }
        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }
        public void OnDialogOpened(IDialogParameters parameters) => Message = parameters.GetValue<string>("message");
        #endregion
    }
}
