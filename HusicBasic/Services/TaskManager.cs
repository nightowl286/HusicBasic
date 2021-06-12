using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using HusicBasic.Models.Tasks;
using HusicBasic.Services.Interfaces;
using Prism.Common;
using Prism.Mvvm;

namespace HusicBasic.Services
{
    public class TaskManager : BindableBase, ITaskManager
    {
        #region Private
        private ObservableCollection<ITask> _Tasks;
        private ITask _Current;
        private int _Count;
        private CancellationTokenSource TokenSource;
        private Dispatcher Dispatcher;
        #endregion

        #region Properties
        public int Count { get => _Count; private set => SetProperty(ref _Count, value); }
        public ReadOnlyObservableCollection<ITask> Tasks => new ReadOnlyObservableCollection<ITask>(_Tasks);
        public ITask Current { get => _Current; private set => SetProperty(ref _Current, value); } 
        public event Action<ITask, TaskResult> TaskFinished;
        public event Action<ITask, IParameters> TaskStarted;
        #endregion
        public TaskManager()
        {
            _Tasks = new ObservableCollection<ITask>();
            _Tasks.CollectionChanged += _Tasks_CollectionChanged;
            Dispatcher = Dispatcher.CurrentDispatcher;
        }

        #region Events
        private void _Tasks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => Count = _Tasks.Count;
        #endregion

        #region Methods
        public void Add(ITask task)
        {
            _Tasks.Add(task);
            TryStartNext();
        }
        private void TryStartNext()
        {
            if (Current == null && Count > 0)
            {
                
                Current = _Tasks[0];
                _Tasks.RemoveAt(0);
                Current.TaskFinished += RaiseTaskFinished;
                Current.TaskStarted += RaiseTaskStarted;
                TokenSource = new CancellationTokenSource();
                Task.Run(() => Current.Start(Dispatcher), TokenSource.Token);
            }
        }
        private void RaiseTaskStarted(ITask task, IParameters parameters)
        {
            TaskStarted?.Invoke(task, parameters);
            Debug.WriteLine($"[Task] | Started | ({task.GetType().Name}) '{task.Name}'");
        }
        private void RaiseTaskFinished(ITask task, TaskResult result)
        {
            Debug.WriteLine($"[Task] | Finished ({result.Success}) | ({task.GetType().Name}) '{task.Name}'");
            task.TaskStarted -= RaiseTaskStarted;
            task.TaskFinished -= RaiseTaskFinished;
            TaskFinished?.Invoke(task, result);
            Current = null;
            TryStartNext();
        }
        #endregion
    }
}
