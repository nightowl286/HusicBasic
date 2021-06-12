using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using HusicBasic.Models.Tasks;
using HusicBasic.Services.Interfaces;
using Prism.Commands;
using Prism.Mvvm;

namespace HusicBasic.ViewModels
{
    public class CurrentTaskViewViewModel : BindableBase
    {
        #region Private
        private ITaskManager Manager;
        private bool _HasTask;
        private TaskViewViewModel _Current;
        #endregion

        #region Properties
        public bool HasTask { get => _HasTask; private set => SetProperty(ref _HasTask, value); }
        public TaskViewViewModel Current { get => _Current; private set => SetProperty(ref _Current, value); }
        #endregion
        public CurrentTaskViewViewModel(ITaskManager manager)
        {
            Manager = manager;
            manager.PropertyChanged += Manager_PropertyChanged;
            UpdateCurrent();
        }

        #region Events
        private void UpdateCurrent()
        {
            Current = Manager.Current == null ? null : new TaskViewViewModel(Manager.Current);
            HasTask = Current != null;
        }
        private void Manager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ITaskManager.Current))
                UpdateCurrent();
        }
        #endregion
    }
}
