using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using HusicBasic.Models;
using HusicBasic.Models.Tasks;
using HusicBasic.Services;
using HusicBasic.Services.Interfaces;
using HusicBasic.Views;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace HusicBasic.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        #region Private
        private SongModel _LastSong;
        private string _Title = "Husic";
        private readonly IHusicPlayer Player;
        private IRegionManager _RegionManager;
        private DelegateCommand<string> _NavigateMain;
        private DelegateCommand _CleanupRegions;
        private readonly IDialogService DialogService;
        private DelegateCommand _RunTest;
        #endregion

        #region Properties
        public string Title { get => _Title; private set => SetProperty(ref _Title, value); }
        public DelegateCommand<string> NavigateMain { get => _NavigateMain; private set => SetProperty(ref _NavigateMain, value); }
        public DelegateCommand CleanupRegions { get => _CleanupRegions; private set => SetProperty(ref _CleanupRegions, value); }
        public DelegateCommand RunTest { get => _RunTest; private set => SetProperty(ref _RunTest, value); }
        #endregion
        public MainWindowViewModel(IHusicPlayer player, IRegionManager regionManager, IDialogService dialogService, IContainerProvider container)
        {
            DialogService = dialogService;
            Player = player;
            player.PropertyChanged += Player_PropertyChanged;
            _RegionManager = regionManager;
            _RegionManager.RegisterViewWithRegion("CurrentTaskRegion", typeof(CurrentTaskView));

            CreateCommands();
           
        }

        #region Events
        private void Player_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IHusicPlayer.CurrentSong))
            {
                if (_LastSong != null) _LastSong.PropertyChanged -= CurrentSong_PropertyChanged;

                _LastSong = Player.CurrentSong;
                _LastSong.PropertyChanged += CurrentSong_PropertyChanged;

                UpdateTitle();
            }
        }
        private void CurrentSong_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SongModel.Title))
                UpdateTitle();
        }
        #endregion

        #region Methods
        private void UpdateTitle()
        {
            Title = Player.CurrentSong == null ? "Husic" : $"Husic | Playing: {Player.CurrentSong.Title}";
        }
        private void CreateCommands()
        {
            NavigateMain = new DelegateCommand<string>(Navigate);
            CleanupRegions = new DelegateCommand(Cleanup);
            RunTest = new DelegateCommand(RunTestFunc);
        }
        private void RunTestFunc()
        {
            DialogService.Show("AddSong", new DialogParameters(), c => { Debug.WriteLine($"Got Res: {c?.Result}"); }, "Popup");
        }
        private void Cleanup()
        {
            _RegionManager.Regions.Remove("MainRegion");
            _RegionManager.Regions.Remove("QueueRegion");
            _RegionManager.Regions.Remove("CurrentTaskRegion");
        }
        private void Navigate(string target)
        {
            if (!string.IsNullOrWhiteSpace(target))
                _RegionManager.RequestNavigate("MainRegion", target, c => {
                    Debug.WriteLine($"MainRegion navigation to '{target}' - Success: {c.Result}");
                    if (c.Result != true) Debug.WriteLine($"Error: {c.Error}\n");
                });
        }
        #endregion
    }
}
