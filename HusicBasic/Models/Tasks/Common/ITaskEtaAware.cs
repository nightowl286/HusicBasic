using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace HusicBasic.Models.Tasks
{
    public interface ITaskEtaAware : ITask
    {
        #region Properties
        TimeSpan ETA { get; }
        bool KnowsETA { get; }
        #endregion
    }
}
