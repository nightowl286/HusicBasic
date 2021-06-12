using System.Windows.Controls;
using Prism.Regions;

namespace HusicBasic.Views.Dialogs.AddSong
{
    /// <summary>
    /// Interaction logic for AddSongDialog
    /// </summary>
    public partial class AddSongDialog : UserControl
    {
        public AddSongDialog(IRegionManager manager)
        {
            InitializeComponent();
            manager.Regions.Remove("AddSongMainRegion");
            RegionManager.SetRegionManager(mainRegion, manager);
        }
    }
}
