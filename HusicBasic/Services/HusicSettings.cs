using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using Prism.Mvvm;
using TNO.BitUtilities;

namespace HusicBasic.Services
{
    public class HusicSettings : BindableBase, IHusicSettings
    {
        #region Private
        private bool DontSave = false;
        private readonly VersionedFileManager<byte, HusicSettings> FileManager = new VersionedFileManager<byte, HusicSettings>();
        private const string PATH = PATH_FOLDER + @"\settings.bin";
        private const string PATH_FOLDER = "data";
        private const string PATH_EXECUTABLES = PATH_FOLDER + @"\executables";
        private Point? _LastMiniPlayerLocation = null;
        private double _LastVolume = 0.5;
        private string _SongStorageLocation = PATH_FOLDER + @"\copies";
        private string _YoutubeExePath = PATH_EXECUTABLES + @"\youtube-dl.exe";
        private string _FFmpegExePath = PATH_EXECUTABLES + @"\ffmpeg.exe";
        private string _FFProbeExePath = PATH_EXECUTABLES + @"\ffprobe.exe";
        #endregion

        #region Properties
        public Point? LastMiniPlayerLocation { get => _LastMiniPlayerLocation; set => SetProperty(ref _LastMiniPlayerLocation, value, Save); }
        public double LastVolume { get => _LastVolume; set => SetProperty(ref _LastVolume, value, Save); }
        public string SongStorageLocation { get => _SongStorageLocation; set => SetProperty(ref _SongStorageLocation, value, Save); }
        public string YoutubeDLExePath { get => _YoutubeExePath; set => SetProperty(ref _YoutubeExePath, value, Save); }
        public string FFMpegExePath { get => _FFmpegExePath; set => SetProperty(ref _FFmpegExePath, value, Save); }
        public string FFProbeExePath { get => _FFProbeExePath; set => SetProperty(ref _FFProbeExePath, value, Save); }
        #endregion
        public HusicSettings() : this(false) { }
        private HusicSettings(bool skipInit)
        {
            if (!skipInit)
            {
                FileManager = new VersionedFileManager<byte, HusicSettings>();
                RegisterFileVersions();
                Reload();
            }
        }

        #region Methods
        #region File Versions
        private void RegisterFileVersions()
        {
            FileManager.Register(1, ReadVersion1, SaveVersion1);
            FileManager.Latest = 1;
        }
        #region Version 1
        private static void SaveVersion1(IAdvancedBitWriter w, HusicSettings inst)
        {
            w.Write(inst._LastVolume);
            w.WriteBool(inst._LastMiniPlayerLocation.HasValue);
            if (inst._LastMiniPlayerLocation.HasValue)
                w.Write(inst._LastMiniPlayerLocation.Value);
            w.WriteString(inst.SongStorageLocation, Encoding.UTF8);
            w.WriteString(inst.YoutubeDLExePath, Encoding.UTF8);
            w.WriteString(inst.FFMpegExePath, Encoding.UTF8);
            w.WriteString(inst.FFProbeExePath, Encoding.UTF8);
        }
        private static HusicSettings ReadVersion1(IAdvancedBitReader r)
        {
            HusicSettings inst = new HusicSettings(true);

            inst._LastVolume = r.Read<double>();
            bool hasMiniLocation = r.ReadBool();
            if (hasMiniLocation)
                inst._LastMiniPlayerLocation = r.Read<Point>();
            inst.SongStorageLocation = r.ReadString(Encoding.UTF8);
            inst.YoutubeDLExePath = r.ReadString(Encoding.UTF8);
            inst.FFMpegExePath = r.ReadString(Encoding.UTF8);
            inst.FFProbeExePath = r.ReadString(Encoding.UTF8);


            return inst;
        }
        #endregion
        #endregion
        public void RestoreDefault()
        {
            DontSave = true;
            LastMiniPlayerLocation = null;
            LastVolume = 0.5;
            DontSave = false;
            Save();
        }
        private void EnsurePath()
        {
            if (!Directory.Exists(PATH_FOLDER))
                Directory.CreateDirectory(PATH_FOLDER);
        }
        public void Reload()
        {
            EnsurePath();
            if (File.Exists(PATH))
            {
                DontSave = true;
                HusicSettings inst = FileManager.Read(PATH);

                foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(inst))
                    prop.SetValue(this, prop.GetValue(inst));

                DontSave = false;
            }
            else RestoreDefault();
        }
        public void Save()
        {
            if (DontSave) return;
            EnsurePath();
            FileManager.Write(PATH, this);
        }
        #endregion
    }
}
