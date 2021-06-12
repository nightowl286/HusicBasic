using System;
using System.Collections.Generic;
using HusicBasic.Models;

namespace HusicBasic.Services
{
    public interface ISongStore : IStoreID<SongModel>
    {
        #region Methods
        SongModel AddNew(string title, Uri path, TimeSpan duration, string youtubeID);
        SongModel AddNew(string title, Uri path, TimeSpan duration);
        SongModel AddNew(string title, Uri path, string youtubeID);
        SongModel AddNew(string title, Uri path);
        bool ContainsByName(string name);
        bool ContainsByYoutubeID(string id);
        #endregion
    }
}
