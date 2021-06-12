using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using HusicBasic.Models.Tasks;
using Prism.Common;

namespace HusicBasic.Services.Interfaces
{
    public interface ITaskManager : INotifyPropertyChanged
    {
        #region Properties
        int Count { get; }
        ReadOnlyObservableCollection<ITask> Tasks { get; }
        event Action<ITask, TaskResult> TaskFinished;
        event Action<ITask, IParameters> TaskStarted;
        ITask Current { get; }
        #endregion

        #region Methods
        void Add(ITask task);
        #endregion
    }
}
