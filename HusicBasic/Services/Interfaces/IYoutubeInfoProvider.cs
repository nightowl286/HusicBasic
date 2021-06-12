using System;
using System.Collections.Generic;
using System.Text;
using HusicBasic.Models;

namespace HusicBasic.Services.Interfaces
{
    public interface IYoutubeInfoProvider
    {
        #region Methods
        YoutubeSongInfo AboutSong(string url);
        #endregion
    }
}
