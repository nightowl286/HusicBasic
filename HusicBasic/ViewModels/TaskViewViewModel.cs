using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using HusicBasic.Models.Tasks;
using Prism.Commands;
using Prism.Mvvm;

namespace HusicBasic.ViewModels
{
    public class TaskViewViewModel : BindableBase
    {
        #region Private
        private FontFamily _TypeIconFont;
        private string _TypeIcon;
        private string _Name;
        private string _Type;
        private ITask _Task;
        private bool _ShowProgress;
        private bool _ShowETA;
        private bool _EtaUnknown;
        private bool _ProgressUnknown;
        private double _Progress;
        private TimeSpan _EtaTime;
        private string _Eta;
        #endregion

        #region Properties
        public string TypeIcon { get => _TypeIcon; private set => SetProperty(ref _TypeIcon, value); }
        public FontFamily TypeIconFont { get => _TypeIconFont; private set => SetProperty(ref _TypeIconFont, value); }
        public string Name { get => _Name; private set => SetProperty(ref _Name, value); }
        public string Type { get => _Type; private set => SetProperty(ref _Type, value); }
        public bool ShowProgress { get => _ShowProgress; private set => SetProperty(ref _ShowProgress, value); }
        public bool ShowETA { get => _ShowETA; private set => SetProperty(ref _ShowETA, value, UpdateEtaString); }
        public bool EtaUnknown { get => _EtaUnknown; private set => SetProperty(ref _EtaUnknown, value, UpdateEtaString); }
        public bool ProgressUnknown { get => _ProgressUnknown; private set => SetProperty(ref _ProgressUnknown, value); }
        public double Progress { get => _Progress; private set => SetProperty(ref _Progress, value); }
        public TimeSpan EtaTime { get => _EtaTime; private set => SetProperty(ref _EtaTime, value, UpdateEtaString); }
        public string Eta { get => _Eta; private set => SetProperty(ref _Eta, value); }
        #endregion
        public TaskViewViewModel(ITask task)
        {
            ChangeTask(task);
        }

        #region Events
        public void ChangeTask(ITask task)
        {
            if (_Task != null)
                _Task.PropertyChanged -= Task_PropertyChanged;

            _Task = task;
            task.PropertyChanged += Task_PropertyChanged;
            Name = task.Name;
            Type = task.Type;
            TypeIcon = task.TypeIcon;
            TypeIconFont = task.TypeIconFont;

            UpdateProgress();
            UpdateETA();
        }
        private void UpdateProgress()
        {
            if (_Task is ITaskProgressAware prog)
            {
                ShowProgress = true;
                Progress = prog.Progress * 100;
                ProgressUnknown = !prog.KnowsProgress;
            }
            else
            {
                ShowProgress = false;
                Progress = 0;
                ProgressUnknown = false;
            }
        }
        private void UpdateEtaString() => Eta = EtaUnknown ? "?" : EtaTime.FormatSimple();
        private void UpdateETA()
        {
            if (_Task is ITaskEtaAware eta)
            {
                ShowETA= true;
                EtaTime = eta.ETA;
                EtaUnknown = !eta.KnowsETA;
            }
            else
            {
                ShowETA = false;
                EtaTime = TimeSpan.Zero;
                EtaUnknown = true;
            }
        }
        private void Task_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ITask.Type))
                Type = _Task.Type;
            else if (e.PropertyName == nameof(ITask.TypeIcon))
                TypeIcon = _Task.TypeIcon;
            else if (e.PropertyName == nameof(ITask.TypeIconFont))
                TypeIconFont = _Task.TypeIconFont;
            else if (e.PropertyName == nameof(ITask.Name))
                Name = _Task.Name;
            else if (ShowETA && (e.PropertyName == nameof(ITaskEtaAware.ETA) || e.PropertyName == nameof(ITaskEtaAware.KnowsETA)))
                UpdateETA();
            else if (ShowProgress && (e.PropertyName == nameof(ITaskProgressAware.Progress) || e.PropertyName == nameof(ITaskProgressAware.KnowsProgress)))
                UpdateProgress();
        }
        #endregion
    }
}
