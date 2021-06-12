using System;
using System.Collections.Generic;
using System.Text;

namespace HusicBasic.Services
{
    public interface IDurationResolver
    {
        #region Methods
        TimeSpan Resolve(Uri path);
        #endregion
    }
}
