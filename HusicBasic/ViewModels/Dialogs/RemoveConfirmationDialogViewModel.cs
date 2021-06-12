using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace HusicBasic.ViewModels.Dialogs
{
    public class RemoveConfirmationDialogViewModel : BindableBase, IDialogAware
    {
        #region Private
        private string _ItemType;
        private string _ItemName;
        private string _Extra;
        private DelegateCommand<string> _GotResult;
        #endregion

        #region Properties
        public string Title => "Remove confirmation";
        public event Action<IDialogResult> RequestClose;
        public string ItemType { get => _ItemType; private set => SetProperty(ref _ItemType, value); }
        public string ItemName { get => _ItemName; private set => SetProperty(ref _ItemName, value); }
        public string Extra { get => _Extra; private set => SetProperty(ref _Extra, value); }
        public DelegateCommand<string> GotResult { get => _GotResult; private set => SetProperty(ref _GotResult, value); }
        #endregion
        public RemoveConfirmationDialogViewModel() => GotResult = new DelegateCommand<string>(ResultGotten);

        #region Methods
        private void ResultGotten(string res)
        {
            ButtonResult result = ButtonResult.None;
            if (res.ToLower() == "no")
                result = ButtonResult.No;
            else if (res.ToLower() == "delete")
                result = ButtonResult.Yes;

            RequestClose?.Invoke(new DialogResult(result));
        }
        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }
        public void OnDialogOpened(IDialogParameters parameters)
        {
            ItemType = parameters.GetValue<string>("type");
            ItemName = parameters.GetValue<string>("name");
            if (parameters.TryGetValue("extra", out string extra))
                Extra = extra;
        }
        #endregion
    }
}
