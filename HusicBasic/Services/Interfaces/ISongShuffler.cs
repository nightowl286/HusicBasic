using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using HusicBasic.Models;

namespace HusicBasic.Services
{
    public interface ISongShuffler
    {
        #region Properties
        string Icon { get; }
        FontFamily IconFont { get; }
        string Name { get; }
        #endregion

        #region Methods
        IList<SongModel> Shuffle(IList<SongModel> songs);
        #endregion
    }
}
