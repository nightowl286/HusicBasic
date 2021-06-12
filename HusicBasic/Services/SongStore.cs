using System;
using System.Collections.Generic;
using System.Text;
using HusicBasic.Models;
using System.IO;
using System.Linq;
using TNO.BitUtilities;
using Prism.Events;
using HusicBasic.Events;
using Prism.Ioc;
using Prism.Mvvm;

namespace HusicBasic.Services
{
    public class SongStore : BindableBase, ISongStore
    {
        #region Variables
        private const string PATH = @"data\songs";
        private List<SongModel> Songs = new List<SongModel>();
        private IDManager IDManager = new IDManager();
        private VersionedFileManager<byte, SongModel> FileManager = new VersionedFileManager<byte, SongModel>();
        private IEventAggregator Events;
        private IContainerProvider Container;
        #endregion
        public SongStore(IEventAggregator events, IContainerProvider container)
        {
            Events = events;
            Container = container;
            RegisterVersions();
        }

        #region Store Methods
        public int GetPotentialCount()
        {
            EnsurePath();
            return Directory.EnumerateFiles(PATH, "*.song").Count();
        }
        public void Add(SongModel song)
        {
            Songs.Add(song);
            RaisePropertyChanged("Songs");
            Save(song);
        }
        public IEnumerable<SongModel> LoadAll()
        {
            EnsurePath();
            foreach (string file in Directory.EnumerateFiles(PATH, "*.song"))
            {
                SongModel song = FileManager.Read(file);
                Songs.Add(song);
                yield return song;
            }
            IDManager = new IDManager(Songs.Select(s => s.ID));
        }
        public SongModel Get(uint id)
        {
            foreach (SongModel song in Songs)
                if (song.ID == id) return song;
            return null;
        }
        public IEnumerable<SongModel> GetAll() => Songs;
        public IEnumerable<SongModel> GetAll(Func<SongModel,bool> condition) => Songs.Where(condition);
        public void FreeReservation(ReservationToken<uint> token) => IDManager.FreeID(token);
        public ReservationToken<uint> ReserveID() => IDManager.ReserveNewID();
        public void Save(SongModel song)
        {
            EnsurePath();
            FileManager.Write(GetPath(song.ID), song);
        }
        public void Remove(SongModel song)
        {
            Events.GetEvent<SongRemovedEvent>().Publish(song);
            IDManager.FreeID(song.ID);
            EnsurePath();

            string idPath = GetPath(song.ID);
            if (File.Exists(idPath))
                File.Delete(idPath);

            if (!string.IsNullOrWhiteSpace(song.YoutubeID))
            {
                string source = song.Path.LocalPath;
                if (File.Exists(source))
                    File.Delete(source);
            }
            Songs.Remove(song);
        }
        public bool Has(uint id)
        {
            EnsurePath();
            return File.Exists(GetPath(id));
        }
        public void TakeReservation(ReservationToken<uint> token) => IDManager.TakeID(token);
        public SongModel AddNew(string title, Uri path, TimeSpan duration) => AddNew(title, path, duration, "");
        public SongModel AddNew(string title, Uri path, TimeSpan duration, string youtubeID)
        {
            var token = IDManager.ReserveNewID();
            SongModel song = new SongModel(this, token.Item, path, duration, title, youtubeID);

            Add(song);
            IDManager.TakeID(token);

            return song;
        }
        public SongModel AddNew(string title, Uri path) => AddNew(title, path, "");
        public SongModel AddNew(string title, Uri path, string youtubeID)
        {
            IDurationResolver resolver = Container.Resolve<IDurationResolver>();
            TimeSpan duration = resolver.Resolve(path);
            return AddNew(title, path, duration, youtubeID);
        }
        public bool ContainsByName(string name)
        {
            string trimmed = name.Trim();
            return Songs.Any(s => s.Title.Trim() == trimmed);
        }
        public bool ContainsByYoutubeID(string id)
        {
            string trimmed = id.Trim();
            return Songs.Any(s => s.YoutubeID.Trim() == trimmed);
        }
        #endregion

        #region Methods
        private string GetPath(uint id)
        {
            EnsurePath();
            return Path.Combine(PATH, $"{id}.song");
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
            FileManager.Register(2, ReadVersion2, WriteVersion2);

            FileManager.Latest = 2;
        }
        #region Reads
        private SongModel ReadVersion1(IAdvancedBitReader r)
        {
            uint id = r.Read<uint>();
            string title = r.ReadString(Encoding.UTF8);
            TimeSpan duration = r.Read<TimeSpan>();
            string path = r.ReadString(Encoding.UTF8);
            return new SongModel(this, id, new Uri(path), duration, title,"");
        }
        private SongModel ReadVersion2(IAdvancedBitReader r)
        {
            uint id = r.Read<uint>();
            string title = r.ReadString(Encoding.UTF8);
            TimeSpan duration = r.Read<TimeSpan>();
            string path = r.ReadString(Encoding.UTF8);
            string yt = r.ReadString(Encoding.UTF8);
            return new SongModel(this, id, new Uri(path), duration, title, yt);
        }
        #endregion
        #region Writes
        private void WriteVersion1(IAdvancedBitWriter w, SongModel song)
        {
            w.Write(song.ID);
            w.WriteString(song.Title, Encoding.UTF8);
            w.Write(song.Duration);
            w.WriteString(song.Path.LocalPath, Encoding.UTF8);
        }
        private void WriteVersion2(IAdvancedBitWriter w, SongModel song)
        {
            WriteVersion1(w, song);
            w.WriteString(song.YoutubeID, Encoding.UTF8);
        }
        #endregion
        #endregion
    }
}
