using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Media;
using HusicBasic.Models;

namespace HusicBasic.Services
{
    public class LinearShuffler : ISongShuffler
    {
        #region Properties
        public string Icon { get; private set; } = "";
        public FontFamily IconFont { get; private set; } = App.GetAssetFont();
        public string Name { get; private set; } = "No shuffle";
        #endregion

        public IList<SongModel> Shuffle(IList<SongModel> songs)
        {
            List<SongModel> c = new List<SongModel>();
            c.AddRange(songs);
            return c;
        }
    }
}
