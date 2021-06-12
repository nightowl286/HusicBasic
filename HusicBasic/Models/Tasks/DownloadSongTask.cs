using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using HusicBasic.Services;
using HusicBasic.Services.Interfaces;

namespace HusicBasic.Models.Tasks
{
    public class DownloadSongTask : TaskBase, ITaskEtaAware, ITaskProgressAware
    {
        #region Private
        private readonly ISongStore SongStore;
        private readonly IHusicSettings Settings;
        private readonly ICLIWrapper Wrapper;
        private string SongName;
        private TimeSpan Duration;
        private string YoutubeID;
        private string SongUrl;
        private string Destination;
        private Dispatcher Dispatcher;
        private int Stage = 0;
        private string FinalLocation;
        private TimeSpan _Eta;
        private double _Progress;
        private bool _KnowsProgress;
        private bool _KnowsETA;
        private Regex DownloadProgressRegex;
        private Regex ConvertProgressRegex;
        private object PleaseLetThisHelp = new object();
        #endregion

        #region Properties
        public TimeSpan ETA { get => _Eta; private set => Dispatcher.Invoke(() => SetProperty(ref _Eta, value)); }
        public double Progress { get => _Progress; private set => Dispatcher.Invoke(() => SetProperty(ref _Progress, value)); }
        public bool KnowsProgress { get => _KnowsProgress; private set => Dispatcher.Invoke(() => SetProperty(ref _KnowsProgress, value)); }
        public bool KnowsETA { get => _KnowsETA; private set => Dispatcher.Invoke(() => SetProperty(ref _KnowsETA, value)); }
        #endregion
        public DownloadSongTask(ISongStore songStore, IHusicSettings settings, ICLIWrapper wrapper) : base("Download song", App.Current.FindResource("Text.Task.Download") as string, "???")
        { 
            SongStore = songStore;
            Settings = settings;
            Wrapper = wrapper;
            DownloadProgressRegex = new Regex("^\\[download]\\s*(\\d+(?:\\.\\d+)?)%\\s*of\\s*(\\d+(?:\\.\\d+)?)(\\w+)\\s*(?:at\\s*(\\d+(?:\\.\\d+)?)(\\w+\\/s))?\\s*(?:ETA|in)\\s+(\\d+:\\d+)");
            ConvertProgressRegex = new Regex("size=\\s*\\d+\\w+\\s*time=(\\d+):(\\d+):(\\d+.\\d+)");
        }

        #region Methods
        protected override void ParametersChanged()
        {
            SongName = Parameters.GetValue<string>("name");
            SongUrl = Parameters.GetValue<string>("url");
            Duration = Parameters.GetValue<TimeSpan>("duration");
            YoutubeID = Parameters.GetValue<string>("youtube-id");

            Name = SongName;
        }
        protected override void StartMain(Dispatcher disp)
        {
            Wrapper.GotLine += Wrapper_GotLine;
            Wrapper.ProcessStopped += Wrapper_ProcessStopped;
            Dispatcher = disp;
            if (!Directory.Exists(Settings.SongStorageLocation))
                Directory.CreateDirectory(Settings.SongStorageLocation);
            Wrapper.Start(Path.Combine(Environment.CurrentDirectory, Settings.YoutubeDLExePath), $"-f bestaudio -o \"{Path.Combine(Environment.CurrentDirectory, Settings.SongStorageLocation, "%(id)s.%(ext)s")}\" --no-continue --no-part --newline --no-color \"{SongUrl}\"", true, false, true);
        }
        private string GetStoragePath(string file) => Path.Combine(Environment.CurrentDirectory, Settings.SongStorageLocation, file);
        private void GenerateFinalPath()
        {
            if (!Directory.Exists(Settings.SongStorageLocation))
            {
                Directory.CreateDirectory(Settings.SongStorageLocation);
                FinalLocation = GetStoragePath($"{SongName}.mp3");
            }
            else
            {
                string name = $"{SongName}.mp3";
                int i = 2;
                while (File.Exists(GetStoragePath(name)))
                    name = $"{SongName} ({i++}).mp3";

                FinalLocation = GetStoragePath(name);
            }
        }
        private void Wrapper_ProcessStopped()
        {
            lock (PleaseLetThisHelp)
            {
                if (Stage == 1)
                {
                    Stage = 2;
                    SongModel song = null;
                    if (File.Exists(Destination))
                    {
                        Dispatcher.Invoke(() => song = SongStore.AddNew(Name, new Uri(FinalLocation), Duration, YoutubeID));
                        File.Delete(Destination);
                        RaiseTaskFinished(new TaskResult(TaskSuccess.Success, new TaskParameters() { { "song", song } }));
                    }
                }
                else if (Stage == 0)
                {
                    Stage = 1;
                    GenerateFinalPath();
                    Type = "Converting";
                    TypeIcon = App.Current.FindResource("Text.Task.Convert") as string;

                    Wrapper.Start(Path.Combine(Environment.CurrentDirectory, Settings.FFMpegExePath), $"-i \"{Destination}\" \"{FinalLocation}\"", true, true, true);
                    Progress = 0;
                    KnowsProgress = true;
                    KnowsETA = false;
                }
            }
        }
        private void Wrapper_GotLine(string data, CLIOutputType type)
        {
            if (Stage == 1)
            {
                if (data == null)
                {
                    Wrapper_ProcessStopped();
                }
                else
                {
                    if (data.StartsWith("size="))
                    {
                        Debug.WriteLine(data);
                        Match match = ConvertProgressRegex.Match(data);
                        if (match.Success)
                        {
                            double hours = double.Parse(match.Groups[1].Value);
                            double minutes = double.Parse( match.Groups[2].Value);
                            double seconds = double.Parse(match.Groups[3].Value);
                            double totalSeconds = (((hours * 60) + minutes) * 60) + seconds;
                            double durSeconds = Duration.TotalSeconds;
                            Progress = totalSeconds / durSeconds;
                        }
                    }
                }

            }
            else if (Stage == 0)
            { 
                if (data == null)
                {
                    Wrapper_ProcessStopped();
                    return;
                }

                if (data.StartsWith("[download] Destination: "))
                {
                    Destination = Path.Combine(Environment.CurrentDirectory, data[24..]);
                    Debug.WriteLine($"[Task] {Type} {Name} | Dest: {Destination}");
                }
                else if (data.StartsWith("[download]"))
                {
                    Match match = DownloadProgressRegex.Match(data);
                    if (match.Success)
                    {
                        Progress = double.Parse(match.Groups[1].Value) / 100d;
                        string[] etaParts = match.Groups[6].Value.Split(':');
                        ETA = TimeSpan.FromSeconds(int.Parse(etaParts[0]) * 60 + int.Parse(etaParts[1]));
                        Debug.WriteLine($"[Task] {Type} {Name} | {(Progress * 100d)}% ETA: {ETA.Minutes}m {ETA.Seconds}s");

                        KnowsETA = true;
                        KnowsProgress = true;
                    }
                }
            }
        }
        #endregion
    }
}
