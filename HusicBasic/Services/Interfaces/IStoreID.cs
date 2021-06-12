using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using HusicBasic.Models;

namespace HusicBasic.Services
{
    public interface IStoreID<T> : INotifyPropertyChanged
    {
        #region Methods
        int GetPotentialCount();
        IEnumerable<T> LoadAll();
        IEnumerable<T> GetAll();
        IEnumerable<T> GetAll(Func<T,bool> condition);
        T Get(uint id);
        void Save(T obj);
        ReservationToken<uint> ReserveID();
        void TakeReservation(ReservationToken<uint> token);
        void FreeReservation(ReservationToken<uint> token);
        void Remove(T obj);
        bool Has(uint id);
        #endregion
    }
}
