using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HusicBasic.Services.Interfaces;
using TNO.Json.Parsing;
using TNO.Json.Runtime;

namespace HusicBasic.Services
{
    public class YoutubeCLI : IYoutubeCLI
    {
        #region Private
        private ICLIWrapper Wrapper;
        private string _ExePath;
        #endregion
        public YoutubeCLI(ICLIWrapper wrapper, IHusicSettings settings)
        {
            Wrapper = wrapper;
            _ExePath = Path.Combine(Environment.CurrentDirectory, settings.YoutubeDLExePath);
        }

        #region Methods
        public JsonValue ParseAsJson(string arguments)
        {
            Wrapper.Start(_ExePath, arguments, true, true, false);
            string output = Wrapper.GetFull().Trim();
            if (output.StartsWith("ERROR")) return null;
            return Parser.QuickParse(output);
        }
        #endregion
    }
}
