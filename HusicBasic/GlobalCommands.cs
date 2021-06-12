using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using Prism.Commands;

namespace HusicBasic
{
    public static class GlobalCommands
    {
        #region Commands
        public static DelegateCommand ShowMiniPlayer { get; set; }
        public static DelegateCommand ShowMainPlayer { get; set; }
        public static DelegateCommand<string> OpenUrl { get; set; }
        public static DelegateCommand<string> OpenYoutubeID { get; set; }
        public static DelegateCommand Exit { get; set; }
        #endregion
    }
}
