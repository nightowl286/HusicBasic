using System;
using System.Collections.Generic;
using System.Text;

namespace HusicBasic.Models.Tasks
{
    public interface ITaskMinMaxAware<T> : ITaskProgressAware where T : unmanaged, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
    {
        #region Properties
        T Min { get; }
        T Max { get; }
        #endregion
    }
}
