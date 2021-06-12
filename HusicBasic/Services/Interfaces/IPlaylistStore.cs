using System;
using System.Collections.Generic;
using System.Text;
using HusicBasic.Models;

namespace HusicBasic.Services
{
    public interface IPlaylistStore : IStoreID<PlaylistModel>
    {
        #region Methods
        public PlaylistModel AddNew(string title, IEnumerable<SongModel> songs);
        #endregion
    }
}
