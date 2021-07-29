using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DryIoc;
using Hardcodet.Wpf.TaskbarNotification;
using HusicBasic.Models.Tasks;
using HusicBasic.Services;
using HusicBasic.Services.Interfaces;
using HusicBasic.ViewModels;
using HusicBasic.ViewModels.Dialogs;
using HusicBasic.Views;
using HusicBasic.Views.Dialogs;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using TNO.Themes;
using TNO.Themes.Transformations.Colors;

namespace HusicBasic
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
       
        #region Variables
        public IHusicPlayer Player;
        private Views.SplashScreen _Splash;
        private TaskbarIcon NotifyIcon;
        public static Window PlayerMiniWindow;
        public static Window PlayerMainWindow;
        #endregion

        #region Prism
        private void SwitchShell(Window newShell)
        {
            RegionManager.SetRegionManager(newShell, Container.Resolve<IRegionManager>());
            RegionManager.UpdateRegions();
            MainWindow = newShell;
        }
        protected override Window CreateShell()
        {

            SetupHusic();

            return null;
        }
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Services
            containerRegistry.RegisterSingleton<ISongStore, SongStore>();
            containerRegistry.RegisterSingleton<IPlaylistStore, PlaylistStore>();
            containerRegistry.Register<ICLIWrapper, CLIWrapper>();
            containerRegistry.Register<IYoutubeCLI, YoutubeCLI>();

            containerRegistry.Register<IDurationResolver, DurationResolver>();
            containerRegistry.Register<IYoutubeInfoProvider, YoutubeInfoProvider>();

            containerRegistry.RegisterSingleton<IHusicSettings, HusicSettings>();
            containerRegistry.RegisterSingleton<IPlayQueue, PlayQueue>();
            containerRegistry.RegisterSingleton<IHusicPlayer, HusicPlayer>();
            containerRegistry.RegisterSingleton<MainPlayerViewModel>();
            containerRegistry.RegisterSingleton<ITaskManager, TaskManager>();

            // Shufflers
            containerRegistry.RegisterSingleton<ISongShuffler, LinearShuffler>();
            containerRegistry.RegisterSingleton<ISongShuffler, RandomShuffler>();

            // View model wire up
            ViewModelLocationProvider.Register<MiniPlayer, MainPlayerViewModel>();
            ViewModelLocationProvider.Register<MainPlayerVolumeControls, MainPlayerViewModel>();
            ViewModelLocationProvider.Register<MainPlayerControls, MainPlayerViewModel>();
            ViewModelLocationProvider.Register<CurrentTaskView, CurrentTaskViewViewModel>();
            //ViewModelLocationProvider.Register<AddSongMethodView, AddSongMethodViewViewModel>();


            containerRegistry.RegisterForNavigation<SongListOverview>();
            containerRegistry.RegisterForNavigation<PlaylistListOverview>();
            containerRegistry.RegisterForNavigation<SongOverview>();
            containerRegistry.RegisterForNavigation<PlaylistOverview>();
            IRegionManager manager = Container.Resolve<IRegionManager>();
            RegisterDialogs(containerRegistry, manager);
            RegisterTasks(containerRegistry);

            manager.RegisterViewWithRegion("QueueRegion", typeof(PlayQueueView));

            CreateGlobalCommands(Container.Resolve<IDialogService>());
        }
        #endregion

        #region Events
        private void AfterSetup()
        {
            CreateNotifyIcon();
        }
        private void CreateNotifyIcon()
        {
            NotifyIcon = new TaskbarIcon();
            NotifyIcon.DataContext = Container.Resolve<MainPlayerViewModel>();
            NotifyIcon.SetResourceReference(TaskbarIcon.StyleProperty, "NotifyIconStyle");
        }
        #endregion

        #region Functions
        private void RegisterTasks(IContainerRegistry container)
        {
            container.Register<AddSongTask>();
            container.Register<DownloadSongTask>();
        }
        private void RegisterDialogs(IContainerRegistry container, IRegionManager manager)
        {
            container.RegisterDialogWindow<PopupDialogWindow>("Popup");
            container.RegisterDialog<ConfirmDeletionDialog, ConfirmDeletionDialogViewModel>("ConfirmDeletion");
        }
        private void CreateGlobalCommands(IDialogService dialogService)
        {
            GlobalCommands.ShowMainPlayer = new DelegateCommand(() => AttemptShowMainWindow());
            GlobalCommands.ShowMiniPlayer = new DelegateCommand(() => AttemptShowWindow<MiniPlayer>(ref PlayerMiniWindow));
            GlobalCommands.OpenUrl = new DelegateCommand<string>(OpenUrl);
            GlobalCommands.OpenYoutubeID = new DelegateCommand<string>(OpenYoutubeUrl);
            GlobalCommands.Exit = new DelegateCommand(() => App.Current.Shutdown());
        }
        private void OpenYoutubeUrl(string id) => OpenUrl($"https://www.youtube.com/watch?v={id}");
        private void OpenUrl(string url) => Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden });
        private void AttemptShowMainWindow()
        {
            if (AttemptShowWindow<MainWindow>(ref PlayerMainWindow))
            {
                SwitchShell(PlayerMainWindow);
            }
        }
        private bool AttemptShowWindow<T>(ref Window window) where T : Window
        {
            if (window?.IsLoaded == true)
            {
                if (window.WindowState == WindowState.Minimized) window.WindowState = WindowState.Normal;
                window.Activate();
                return false;
            }
            else
            {
                window = Container.Resolve<T>();
                window.Show();
                return true;
            }
        }
        public static FontFamily GetAssetFont() => App.Current.FindResource("fntAssets") as FontFamily;
        private void SetupHusic()
        {
            ReloadImages();

            LoadDefaultTheme();
            ShowSplash();
        }
        private void ReloadImages()
        {
            var dct = Resources.MergedDictionaries.First(r => r.Contains("LogoFrame"));
            string logoPath = Path.Combine(Environment.CurrentDirectory, "Resources", "Logo");
            dct["LogoFrame"] = LoadBitmap(Path.Combine(logoPath, "frame.png"));
            dct["LogoCoverLeft"] = LoadBitmap(Path.Combine(logoPath, "coverLeft.png"));
            dct["LogoCoverRight"] = LoadBitmap(Path.Combine(logoPath, "coverRight.png"));
        }
        private BitmapImage LoadBitmap(string path)
        {
            BitmapImage bmp = new BitmapImage();
            bmp.DownloadFailed += Bmp_DownloadFailed;
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnDemand;
            bmp.UriSource = new Uri(path);
            bmp.EndInit();
            return bmp;
        }

        private void Bmp_DownloadFailed(object sender, ExceptionEventArgs e)
        {
            Debug.WriteLine($"[Error : reloading image at startup] {e.ErrorException.Message}");
        }

        private void ShowSplash()
        {
            _Splash = Container.Resolve<Views.SplashScreen>();
            _Splash.Show();
            if (_Splash.DataContext is SplashScreenViewModel splashModel)
            {
                splashModel.TasksFinished += SplashModel_TasksFinished;
                splashModel.AsyncStartTasks();
            }
        }
        private void SplashModel_TasksFinished()
        {
            Dispatcher.Invoke(() => {
                AttemptShowMainWindow();

                _Splash.Close();
                _Splash = null;
                AfterSetup();
            });
        }
        private void LoadDefaultTheme()
        {
            ColorTransformations.Init();
            ThemeManager.AddTransformationsWithReflection();
            ThemeManager.RegisterBaseDuplications();

            ThemeManager.SetDefault(Resources.MergedDictionaries[0], Current.Resources, 0);
            ThemeManager.SaveTransformations("default", Resources.MergedDictionaries[0], new List<string>() { "Color.Accent", "Color.Background", "Color.Text", "Color.Link" });

            ThemeManager.SwitchTheme(new Dictionary<string, object>() {
                { "Color.Accent", ColorConverter.ConvertFromString("#ad3b3b") },
                { "Color.Background", ColorConverter.ConvertFromString("#171717") },
                { "Color.Text", ColorConverter.ConvertFromString("#d7d7d7") },
                {"Color.Link", ColorConverter.ConvertFromString("#1e5a99") }
            });

        }
        #endregion
    }
}