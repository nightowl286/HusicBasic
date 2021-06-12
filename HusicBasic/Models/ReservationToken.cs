using System;
using System.Collections.Generic;
using System.Text;

namespace HusicBasic.Models
{
    public class ReservationToken<T>
    {
        #region Properties
        private readonly object SourceRef;
        public T Item { get; private set; }
        #endregion
        public ReservationToken(object sourceRef, T item)
        {
            SourceRef = sourceRef;
            Item = item;
        }

        #region Methods
        public bool IsFromSource(object sourceRef) => SourceRef == sourceRef;
        #endregion
    }
}
