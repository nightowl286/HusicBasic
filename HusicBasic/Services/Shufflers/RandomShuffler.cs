using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Media;
using HusicBasic.Models;

namespace HusicBasic.Services
{
    public class RandomShuffler : ISongShuffler
    {
        #region Properties
        public string Icon { get; private set; } = "";
        public FontFamily IconFont { get; private set; } = App.GetAssetFont();
        public string Name { get; private set; } = "Random shuffle";
        #endregion

        #region Methods
        public IList<SongModel> Shuffle(IList<SongModel> songs)
        {
            Collection<SongModel> temp = new Collection<SongModel>();
            temp.AddRange(songs);

            Collection<SongModel> shuffled = new Collection<SongModel>();
            Random rand = new Random();
            while (temp.Count > 0)
            {
                int i = rand.Next(0, temp.Count);
                shuffled.Add(temp[i]);
                temp.RemoveAt(i);
            }
            return shuffled;
        }
        #endregion
    }
}
