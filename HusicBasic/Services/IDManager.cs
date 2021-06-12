using System;
using System.Collections.Generic;
using System.Text;
using HusicBasic.Models;
using System.Linq;

namespace HusicBasic.Services
{
    public class IDManager
    {
        #region Fields
        private object ReservationRef = new object();
        private uint NextID = 0;
        private HashSet<uint> FreedIDs = new HashSet<uint>();
        private Dictionary<uint, WeakReference<ReservationToken<uint>>> Reserved = new Dictionary<uint, WeakReference<ReservationToken<uint>>>();
        #endregion
        public IDManager() { }
        public IDManager(IEnumerable<uint> takenIDs)
        {
            takenIDs = takenIDs.OrderBy(n => n);
            foreach (uint id in takenIDs)
            {
                if (id == NextID)
                    NextID++;
                else
                {
                    FreedIDs.Add(id);
                    NextID = id + 1;
                }
            }
        }

        #region Methods
        public ReservationToken<uint> ReserveNewID()
        {
            uint id = GetNewID();
            ReservationToken<uint> token = new ReservationToken<uint>(ReservationRef, id);
            Reserved.Add(id, new WeakReference<ReservationToken<uint>>(token));
            return token;
        }
        public void TakeID(ReservationToken<uint> token)
        {
            CheckToken(token);
            Reserved.Remove(token.Item);
            CleanReservations();
        }
        public void FreeID(ReservationToken<uint> token)
        {
            TakeID(token);
            FreeID(token.Item);
        }
        private void CheckToken(ReservationToken<uint> token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            if (!Reserved.ContainsKey(token.Item) || !token.IsFromSource(ReservationRef))
                throw new ArgumentException("This token was not given out by this instance of the id manager.", nameof(token));
        }
        private uint GetNewID()
        { 
            if (FreedIDs.Count > 0)
            {
                uint id = FreedIDs.First();
                FreedIDs.Remove(id);
                return id;
            }
            return NextID++;
        }
        public void FreeID(uint id)
        {
            if (id >= NextID || FreedIDs.Contains(id)) throw new ArgumentException($"The ID '{id}' has not been taken by this manager.", nameof(id));

            if (id == NextID - 1)
                NextID--;
            else
                FreedIDs.Add(id);

            CleanFree();
        }
        public void CleanReservations()
        {
            foreach(var pair in Reserved)
            {
                if (!pair.Value.TryGetTarget(out _))
                {
                    Reserved.Remove(pair.Key);
                    FreeID(pair.Key);
                }
            }
        }
        public void CleanFree()
        {
            while (FreedIDs.Contains(NextID - 1))
                FreedIDs.Remove(NextID--);
        }
        #endregion
    }
}
