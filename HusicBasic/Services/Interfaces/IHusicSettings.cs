using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;

namespace HusicBasic.Services
{
    public interface IHusicSettings : INotifyPropertyChanged
    {
        #region Properties
        double LastVolume { get; set; }
        Point? LastMiniPlayerLocation { get; set; }
        string SongStorageLocation { get; set; }
        string YoutubeDLExePath { get; set; }
        string FFMpegExePath { get; set; }
        string FFProbeExePath { get; set; }
        #endregion

        #region Methods
        void RestoreDefault();
        void Reload();
        void Save();
        #endregion
    }
}
