using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using HusicBasic.Models;
using Prism.Events;
using Prism.Mvvm;
using TNO.BitUtilities;

namespace HusicBasic.Services
{
    public class PlaylistStore : BindableBase, IPlaylistStore
    {
        #region Variables
        private const string PATH = @"data\playlists";
        private List<PlaylistModel> Playlists = new List<PlaylistModel>();
        private IDManager IDManager = new IDManager();
        private VersionedFileManager<byte, PlaylistModel> FileManager = new VersionedFileManager<byte, PlaylistModel>();
        private ISongStore SongStore;
        private IEventAggregator Events;
        #endregion
        public PlaylistStore(ISongStore songStore, IEventAggregator events)
        {
            SongStore = songStore ?? throw new ArgumentNullException(nameof(songStore));
            Events = events;
            RegisterVersions();
        }

        #region Store Methods
        public int GetPotentialCount()
        {
            EnsurePath();
            return Directory.EnumerateFiles(PATH, "*.playlist").Count();
        }
        
        public IEnumerable<PlaylistModel> LoadAll()
        {
            EnsurePath();
            foreach (string file in Directory.EnumerateFiles(PATH, "*.playlist"))
            {
                PlaylistModel playlist = FileManager.Read(file);
                Playlists.Add(playlist);
                yield return playlist;
            }
            IDManager = new IDManager(Playlists.Select(s => s.ID));
        }
        public IEnumerable<PlaylistModel> GetAll() => Playlists;
        public IEnumerable<PlaylistModel> GetAll(Func<PlaylistModel, bool> condition) => Playlists.Where(condition);
        public PlaylistModel Get(uint id)
        {
            foreach (PlaylistModel playlist in Playlists)
                if (playlist.ID == id)
                    return playlist;
            throw new ArgumentException($"No playlist with the id '{id}'.",nameof(id));
        }
        public void Save(PlaylistModel playlist)
        {
            EnsurePath();
            FileManager.Write(GetPath(playlist.ID), playlist);
        }
        public ReservationToken<uint> ReserveID() => IDManager.ReserveNewID();
        public void FreeReservation(ReservationToken<uint> token) => IDManager.FreeID(token);
        public void TakeReservation(ReservationToken<uint> token) => IDManager.TakeID(token);
        public void Remove(PlaylistModel playlist)
        {
            IDManager.FreeID(playlist.ID);
            EnsurePath();
            string path = GetPath(playlist.ID);
            if (File.Exists(path))
                File.Delete(path);
        }
        public bool Has(uint id)
        {
            EnsurePath();
            return File.Exists(GetPath(id));
        }
        #endregion

        #region Methods
        public bool ContainsByName(string name)
        {
            string trimmed = name.Trim();
            return Playlists.Any(s => s.Title.Trim() == trimmed);
        }
        public PlaylistModel AddNew(string title, IEnumerable<SongModel> songs)
        {
            var token = IDManager.ReserveNewID();

            PlaylistModel playlist = new PlaylistModel(this, Events, token.Item, title, new ObservableCollection<SongModel>(songs));
            Save(playlist);
            Playlists.Add(playlist);

            IDManager.TakeID(token);

            return playlist;
        }
        private string GetPath(uint id)
        {
            EnsurePath();
            return Path.Combine(PATH, $"{id}.playlist");
        }
        private void EnsurePath()
        {
            if (!Directory.Exists(PATH))
                Directory.CreateDirectory(PATH);
        }
        #endregion

        #region Versioned File methods
        private void RegisterVersions()
        {
            FileManager.Register(1, ReadVersion1, WriteVersion1);

            FileManager.Latest = 1;
        }
        #region Version 1
        private PlaylistModel ReadVersion1(IAdvancedBitReader r)
        {
            uint id = r.Read<uint>();
            string title = r.ReadString(Encoding.UTF8);
            int count = r.Read<int>();
            List<SongModel> songs = new List<SongModel>();
            for(int i = 0; i < count;i++)
            {
                uint songId = r.Read<uint>();
                SongModel song = SongStore.Get(songId);
                songs.Add(song);
            }

            return new PlaylistModel(this, Events, id, title, new ObservableCollection<SongModel>(songs));
        }
        private void WriteVersion1(IAdvancedBitWriter w, PlaylistModel playlist)
        {
            w.Write(playlist.ID);
            w.WriteString(playlist.Title, Encoding.UTF8);
            w.Write(playlist.Count);
            foreach (SongModel song in playlist.Songs)
                w.Write(song.ID);
        }
        #endregion
        #endregion

    }
}
