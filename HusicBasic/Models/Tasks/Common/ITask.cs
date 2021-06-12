using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using Prism.Common;

namespace HusicBasic.Models.Tasks
{
    public interface ITask : INotifyPropertyChanged
    {
        #region Properties
        string Name { get; }
        string Type { get; }
        FontFamily TypeIconFont { get; }
        string TypeIcon { get; }
        IParameters Parameters { get; set; }
        event Action<ITask, TaskResult> TaskFinished;
        event Action<ITask, IParameters> TaskStarted;
        #endregion

        #region Methods
        void Start(Dispatcher dispatcher);
        void RaiseTaskStarted(IParameters parameters);
        void RaiseTaskFinished(TaskResult result);
        #endregion
    }
}
