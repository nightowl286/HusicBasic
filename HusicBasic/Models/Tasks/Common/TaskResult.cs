using System;
using System.Collections.Generic;
using System.Text;
using Prism.Common;

namespace HusicBasic.Models.Tasks
{
    public enum TaskSuccess
    {
        Success,
        Failed,
        Cancelled,
    }

    public class TaskResult
    {
        #region Properties
        public TaskSuccess Success { get; private set; }
        public IParameters Parameters { get; private set; }
        #endregion
        public TaskResult(TaskSuccess success, IParameters parameters)
        {
            Success = success;
            Parameters = parameters;
        }
    }
}
