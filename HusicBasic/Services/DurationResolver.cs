using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HusicBasic.Services
{
    public class DurationResolver : IDurationResolver
    {
        #region Private
        private ICLIWrapper CLIWrapper;
        private string FFProbeLocation;
        #endregion
        public DurationResolver(ICLIWrapper cliwrapper, IHusicSettings settings)
        {
            CLIWrapper = cliwrapper;
            FFProbeLocation = Path.Combine(Environment.CurrentDirectory, settings.FFProbeExePath);
        }

        #region Methods
        public TimeSpan Resolve(Uri path)
        {
            CLIWrapper.Start(FFProbeLocation, $"-i \"{path.LocalPath}\" -show_entries format=duration -v quiet -of csv=p=0", true, true, false);
            string output = CLIWrapper.GetFull().Trim();
            if (double.TryParse(output, out double seconds))
                return TimeSpan.FromSeconds(seconds);
            return TimeSpan.Zero;
        }
        #endregion
    }
}
