using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Threading;
using HusicBasic.Services;
using Prism.Common;
using Prism.Mvvm;

namespace HusicBasic.Models.Tasks
{
    public class AddSongTask : TaskBase
    {
        #region Private
        private readonly ISongStore SongStore;
        private readonly IHusicSettings Settings;

        private string SongSource;
        private bool CopySource;
        private string SongName;
        private string SongYtID;
        #endregion
        public AddSongTask(ISongStore songStore, IHusicSettings settings) : base("Add song","?","???")
        {
            SongStore = songStore;
            Settings = settings;
        }


        #region Methods
        protected override void StartMain(Dispatcher disp)
        {
            if (File.Exists(SongSource))
            {
                if (CopySource)
                {
                    if (!Directory.Exists(Settings.SongStorageLocation))
                        Directory.CreateDirectory(Settings.SongStorageLocation);

                    string name = Path.GetFileName(SongSource);
                    string path = Path.Combine(Environment.CurrentDirectory, Settings.SongStorageLocation, name);
                    if (!File.Exists(path))
                        File.Copy(SongSource, path);
                    SongSource = path;
                }
                SongModel song = null;
                disp.Invoke(() => {
                    song = SongStore.AddNew(SongName, new Uri(SongSource), SongYtID);
                });
                RaiseTaskFinished(new TaskResult(TaskSuccess.Success, new TaskParameters()
                {
                    {"song",song }
                }));
            }
            else RaiseTaskFinished(TaskSuccess.Failed, null);
        }
        protected override void ParametersChanged()
        {
            if (Parameters != null)
            {
                Parameters.TryGetValue("name", out SongName);
                Parameters.TryGetValue("source", out SongSource);
                if (!Parameters.TryGetValue("copy", out CopySource))
                    CopySource = false;
                if (!Parameters.TryGetValue("yt-id", out SongYtID))
                    SongYtID = "";


                Name = SongName;
            }
        }
        #endregion
    }
}
