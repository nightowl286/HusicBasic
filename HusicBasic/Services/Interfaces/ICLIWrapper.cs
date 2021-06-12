using System;
using System.Collections.Generic;
using System.Text;

namespace HusicBasic.Services
{
    public enum CLIOutputType
    {
        StandardOutput,
        StandardError,
    }

    public delegate void GotOutputHandler(string data, CLIOutputType type);

    public interface ICLIWrapper
    {
        #region Properties
        event GotOutputHandler GotLine;
        event GotOutputHandler GotAll;
        event Action ProcessStopped;
        bool RedirectOutput { get; }
        bool RedirectError { get; }
        #endregion

        #region Methods
        void Start(string path, string arguments, bool redirectOutput, bool redirectError, bool readLines);
        void WaitForExit();
        string GetFull();
        string GetFullOutput();
        string GetFullError();
        #endregion
    }
}
