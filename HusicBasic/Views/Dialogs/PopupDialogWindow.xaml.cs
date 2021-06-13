using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace HusicBasic.Views
{
    /// <summary>
    /// Interaction logic for PopupDialogWindow.xaml
    /// </summary>
    public partial class PopupDialogWindow : Window, IDialogWindow
    {
        #region Properties
        public IDialogResult Result { get; set; }
        private bool IsAnimating = true;
        #endregion
        public PopupDialogWindow()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        #region Events
        private void Owner_LocationChanged(object sender, EventArgs e) => UpdateLocation();
        private void Owner_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateSize();
        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsAnimating && Mouse.Captured == this)
            {
                Mouse.Capture(null);
                AttemptClose();
            }
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsAnimating && Mouse.Captured == null)
                Mouse.Capture(this);

        }
        private void Sb_Completed(object sender, EventArgs e)
        {
            IsAnimating = false;
            Owner.StateChanged += Owner_StateChanged;
            Owner.LocationChanged += Owner_LocationChanged;
            Owner.SizeChanged += Owner_SizeChanged;
            (Content as UIElement).Opacity = 1;
            //this.WindowState = Owner.WindowState;
            UpdateSize();
            UpdateLocation();
        }

        private void Owner_StateChanged(object sender, EventArgs e)
        {
            UpdateSize();
            UpdateLocation();
            //this.WindowState = Owner.WindowState;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this.WindowState = Owner.WindowState;
            StarteAnimateIn();
        }
        #endregion

        #region Methods
        private void StarteAnimateIn()
        {
            DoubleAnimation width = this.FindResource("animateInWidth") as DoubleAnimation;
            DoubleAnimation left = this.FindResource("animateInLeft") as DoubleAnimation;
            DoubleAnimation top = this.FindResource("animateInTop") as DoubleAnimation;
            DoubleAnimation height = this.FindResource("animateInHeight") as DoubleAnimation;
            DoubleAnimation opacity = this.FindResource("animateInOpacity") as DoubleAnimation;
            DoubleAnimation contentOpacity = this.FindResource("animateInContentOpacity") as DoubleAnimation;

            this.Width = 0;
            this.Height = 0;
            Size size = GetDesiredSize();
            Point location = GetDesiredLocation();
            this.Left = location.X + (size.Width * .5);
            this.Top = location.Y + (size.Height * .5);
            (Content as UIElement).Opacity = 0;

            width.To = size.Width;
            height.To = size.Height;
            top.To = location.Y;
            left.To = location.X;

            Storyboard sb = new Storyboard()
            {
                FillBehavior = FillBehavior.Stop,
                Children = new TimelineCollection
                {
                    opacity, left, top, width, height, contentOpacity
                }
            };
            sb.Completed += Sb_Completed;
            sb.Begin(this);
        }
        private void AttemptClose()
        {
            if (IsAnimating) return;
            if (Content is FrameworkElement elem && elem?.DataContext is IDialogAware aware)
            { 
                if (aware.CanCloseDialog())
                    Close();
            }
            else
                Close();
        }
        private FrameworkElement FirstElement() => Owner.Content as FrameworkElement;
        private void UpdateSize()
        {
            Size desired = GetDesiredSize();

            this.Width = desired.Width;
            this.Height = desired.Height;
        }
        private void UpdateLocation()
        {
            Point desired = GetDesiredLocation();
            this.Left = desired.X;
            this.Top = desired.Y;
        }
        private Point GetDesiredLocation()
        {
            Point p = new Point(SystemParameters.BorderWidth * SystemParameters.Border,
                SystemParameters.WindowCaptionHeight  + (SystemParameters.BorderWidth * SystemParameters.Border));

            if (Owner.WindowState == WindowState.Normal)
            {
                p.Offset(Owner.Left + SystemParameters.WindowResizeBorderThickness.Left + SystemParameters.FixedFrameVerticalBorderWidth,
                    Owner.Top + SystemParameters.WindowResizeBorderThickness.Top + SystemParameters.FixedFrameHorizontalBorderHeight);
            }
            else
            {
                var loc = Owner.GetAbsolutePosition();
                p.Offset(loc.X, loc.Y);
            }
            return p;
        }
        private Size GetDesiredSize()
        {
            var elem = FirstElement();

            return new Size(elem.ActualWidth + elem.Margin.Left + elem.Margin.Right,
                elem.ActualHeight + elem.Margin.Top + elem.Margin.Bottom);
        }
        #endregion

        
    }
}
