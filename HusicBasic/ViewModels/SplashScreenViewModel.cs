using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HusicBasic.Models;
using HusicBasic.Services;
using Prism.Mvvm;

namespace HusicBasic.ViewModels
{
    public class SplashScreenViewModel : BindableBase
    {
        #region Private
        #region Animation
        public const double SlopeAngle = -12.5;
        public const double OffsetAmount = 2;
        private const double RadiansMult = Math.PI / 180d;
        private const double StartMult = -1.5d;
        private double _OffsetX = Math.Cos(SlopeAngle * RadiansMult) * OffsetAmount;
        private double _OffsetY = Math.Sin(SlopeAngle * RadiansMult) * OffsetAmount;
        #endregion
        private readonly ISongStore SongStore;
        private readonly IPlaylistStore PlaylistStore;
        private readonly IHusicSettings Settings;
        private double _Max = 1;
        private double _Progress = 0;
        private string _ProgressText = "0%";
        private string _ProgressStatus = "Loading...";
        private bool _ProgressUnknown = true;
        public event Action TasksFinished;
        #endregion

        #region Properties
        #region Animation
        public double LeftOffsetX => _OffsetX;
        public double RightOffsetX => -_OffsetX;
        public double OffsetY => _OffsetY;
        public double LeftStartX => _OffsetX * StartMult;
        public double RightStartX => -_OffsetX * StartMult;
        public double StartY => _OffsetY * StartMult;
        #endregion
        public double Max { get => _Max; private set => SetProperty(ref _Max, value, UpdateProgressText); }
        public double Progress { get => _Progress; private set => SetProperty(ref _Progress, value, UpdateProgressText); }
        public string ProgressStatus { get => _ProgressStatus; private set => SetProperty(ref _ProgressStatus, value); }
        public string ProgressText { get => _ProgressText; private set => SetProperty(ref _ProgressText, value); }
        public bool ProgressUnknown { get => _ProgressUnknown; private set => SetProperty(ref _ProgressUnknown, value, UpdateProgressText); }
        #endregion
        public SplashScreenViewModel(ISongStore songStore, IPlaylistStore playlistStore, IHusicSettings settings)
        {
            SongStore = songStore;
            PlaylistStore = playlistStore;
            Settings = settings;
        }

        #region Methods
        private void UpdateProgressText() => ProgressText = ProgressUnknown ? string.Empty : Max == 0 ? "0%" : $"{Math.Clamp(Progress / Max * 100d, 0d, 100d):n0}%";
        public Task AsyncStartTasks() => Task.Run(StartTasks);
        private void IncrementProgress(string newStatus)
        {
            App.Current.Dispatcher.Invoke(() => {
                Progress++;
                ProgressStatus = newStatus;
            });
        }
        private void InitInfo(double prog, double max, string status, bool unknown)
        {
            App.Current.Dispatcher.Invoke(() => {
                Progress = prog;
                Max = max;
                ProgressStatus = status;
                ProgressUnknown = unknown;
            });
        }
        private void RunTask(Action task)
        {
            try
            {
                task();
            }
            catch(Exception ex) 
            {
                Debug.WriteLine($"Error while running startup task {task}: {ex.Message}");
            }
        }
        private void StartTasks()
        {
            RunTask(CreateInitialDirectories);

            RunTask(Cleanup);

            RunTask(LoadSongs);
            RunTask(CleanupAfterSongsLoaded);

            RunTask(LoadPlaylists);

            TasksFinished?.Invoke();
        }
        private void CreateInitialDirectories()
        {
            TryCreateFolder(Settings.SongStorageLocation);
            TryCreateFromFile(Settings.FFMpegExePath);
            TryCreateFromFile(Settings.FFProbeExePath);
            TryCreateFromFile(Settings.YoutubeDLExePath);
        }
        private void TryCreateFromFile(string file)
        {
            string dir = Path.GetDirectoryName(file);
            TryCreateFolder(dir);
        }
        private void TryCreateFolder(string folder)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
        }
        private void LoadSongs()
        {
            InitInfo(0, SongStore.GetPotentialCount(), "Loading songs...", false);
            foreach (SongModel song in SongStore.LoadAll())
                IncrementProgress($"Loaded song - {song.Title}");
        }
        private void LoadPlaylists()
        {
            int max = PlaylistStore.GetPotentialCount();
            InitInfo(0, max, "Loading playlists...", false);

            foreach (PlaylistModel playlist in PlaylistStore.LoadAll())
                IncrementProgress($"Loaded playlist - {playlist.Title}");
        }
        private void Cleanup()
        {
            InitInfo(0, 1, "Cleaning up...", true);
            foreach(string songFile in Directory.EnumerateFiles(Settings.SongStorageLocation))
            {
                if (!songFile.EndsWith(".mp3"))
                    File.Delete(songFile);
            }
        }
        private void CleanupAfterSongsLoaded()
        {
            InitInfo(0, 1, "Cleaning up...", true);

            foreach (string songFile in Directory.EnumerateFiles(Settings.SongStorageLocation))
            {
                string fullPath = Path.Combine(Environment.CurrentDirectory, songFile);
                if (songFile.EndsWith(".mp3") && SongStore.GetAll().All(song => song.Path.LocalPath?.Equals(fullPath) != true))
                    File.Delete(songFile);
            }
        }
        #endregion
    }
}
