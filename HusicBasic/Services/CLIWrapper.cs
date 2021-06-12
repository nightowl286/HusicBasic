using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace HusicBasic.Services
{
    public class CLIWrapper : ICLIWrapper
    {
        #region Private
        private Process Proc;
        #endregion

        #region Events
        public event GotOutputHandler GotLine;
        public event GotOutputHandler GotAll;
        public event Action ProcessStopped;
        #endregion

        #region Properties
        public bool RedirectOutput { get; private set; }
        public bool RedirectError { get; private set; }
        #endregion

        #region Methods
        public void Start(string path, string arguments, bool redirectOutput, bool redirectError, bool readLines)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(path, arguments)
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = redirectOutput,
                RedirectStandardError = redirectError,
            };

            Debug.WriteLine($"[CLI] STARTED | {Path.GetFileName(path)} | {arguments}");

            RedirectError = redirectError;
            RedirectOutput = redirectOutput;

            Proc = Process.Start(startInfo);

            if (readLines)
            {
                if (redirectOutput)
                {
                    Proc.OutputDataReceived += Proc_OutputDataReceived;
                    Proc.BeginOutputReadLine();
                }
                if (redirectError)
                {
                    Proc.ErrorDataReceived += Proc_ErrorDataReceived;
                    Proc.BeginErrorReadLine();
                }
            }

        }
        public void WaitForExit()
        {
            Proc.WaitForExit();
            ProcessStopped?.Invoke();
        }
        public string GetFullOutput() => GetFull(RedirectOutput, Proc.StandardOutput, CLIOutputType.StandardOutput);
        public string GetFullError() => GetFull(RedirectError, Proc.StandardError, CLIOutputType.StandardError);
        public string GetFull() => GetFullOutput() + GetFullError();
        private string GetFull(bool allow, StreamReader reader, CLIOutputType type)
        {
            if (allow)
            {
                string all = reader.ReadToEnd();
                GotAll?.Invoke(all, type);
                return all;
            }
            return "";
        }
        private void Proc_ErrorDataReceived(object sender, DataReceivedEventArgs e) => GotLine?.Invoke(e.Data, CLIOutputType.StandardError);
        private void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e) => GotLine?.Invoke(e.Data, CLIOutputType.StandardOutput);
        #endregion
    }
}
