using System;
using System.Windows;
using System.Windows.Input;
using HusicBasic.Services;

namespace HusicBasic.Views
{
    /// <summary>
    /// Interaction logic for MiniPlayer.xaml
    /// </summary>
    public partial class MiniPlayer : Window
    {
        #region Variables
        private bool MovedSinceSnap = false;
        private readonly IHusicSettings Settings;
        #endregion
        public MiniPlayer(IHusicSettings settings)
        {
            InitializeComponent();
            Settings = settings;
            if (settings.LastMiniPlayerLocation.HasValue)
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                Left = settings.LastMiniPlayerLocation.Value.X;
                Top = settings.LastMiniPlayerLocation.Value.Y;
            }
        }

        #region Events
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var target = SystemParameters.SwapButtons ? MouseButton.Right : MouseButton.Left;
            if (e.ChangedButton == target && e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
        private void Window_LocationChanged(object sender, EventArgs e) => MovedSinceSnap = true;
        private void Snap()
        {
            Point topLeft = new Point(Left, Top);
            Vector size = new Vector(ActualWidth, ActualHeight);

            Point center = Point.Add(topLeft, size * .5);
            center.Offset(-(center.X % 10), -(center.Y % 10));

            center.Offset(size.X * -0.5d, size.Y * -0.5d);

            Left = center.X;
            Top = center.Y;

            Settings.LastMiniPlayerLocation = new Point(Left, Top);
        }
        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (MovedSinceSnap) Snap();
        }
        #endregion

    }
}
