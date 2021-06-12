using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Media;
using System.Windows.Threading;
using Prism.Common;
using Prism.Mvvm;

namespace HusicBasic.Models.Tasks
{
    public abstract class TaskBase : BindableBase, ITask
    {
        #region Private
        private string _Name;
        private string _Type;
        private FontFamily _TypeIconFont;
        private string _TypeIcon;
        private IParameters _Parameters;
        #endregion

        #region Properties
        public string Name { get => _Name; protected set => SetProperty(ref _Name, value); }
        public string Type { get => _Type; protected set => SetProperty(ref _Type, value); }
        public IParameters Parameters { get => _Parameters; set => TryChangeParameters(value); }
        public FontFamily TypeIconFont { get => _TypeIconFont; protected set => SetProperty(ref _TypeIconFont, value); }
        public string TypeIcon { get => _TypeIcon; protected set => SetProperty(ref _TypeIcon, value); }
        public event Action<ITask, TaskResult> TaskFinished;
        public event Action<ITask, IParameters> TaskStarted;
        #endregion
        protected TaskBase(string type,string typeIcon, string name)
        {
            Type = type;
            TypeIcon = typeIcon;
            TypeIconFont = App.Current.FindResource("fntAssets") as FontFamily;
            Name = name;
        }

        #region Methods
        private void TryChangeParameters(IParameters value)
        {
            if (Parameters == null)
            {
                SetProperty(ref _Parameters, value);
                ParametersChanged();
            }
        }
        public void RaiseTaskFinished(TaskResult result) => TaskFinished?.Invoke(this, result);
        public void RaiseTaskFinished(TaskSuccess success, IParameters parameters) => TaskFinished?.Invoke(this, new TaskResult(success, parameters));
        public void RaiseTaskStarted(IParameters parameters) => TaskStarted?.Invoke(this, parameters);
        public void Start(Dispatcher dispatcher)
        {
            RaiseTaskStarted(Parameters);

            StartMain(dispatcher);
        }
        protected abstract void ParametersChanged();
        protected abstract void StartMain(Dispatcher disp);
        #endregion
    }
}
