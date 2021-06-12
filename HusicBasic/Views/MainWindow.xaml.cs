using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using HusicBasic.Services;
using Prism.Regions;
using TNO.Themes;

namespace HusicBasic.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(IRegionManager manager)
        {
            InitializeComponent();
            manager.Regions.Remove("MainRegion");
            RegionManager.SetRegionName(mainRegionControl, "MainRegion");
            RegionManager.SetRegionManager(mainRegionControl, manager);
            manager.RequestNavigate("MainRegion", nameof(SongListOverview));
        }
    }
}
