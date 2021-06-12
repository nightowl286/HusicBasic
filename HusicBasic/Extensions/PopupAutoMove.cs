using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace HusicBasic
{
    public static class PopupAutoMove
    {
        #region Properties
        public static readonly DependencyProperty AutoMoveProperty = DependencyProperty.RegisterAttached("AutoMove", typeof(bool), typeof(PopupAutoMove), new FrameworkPropertyMetadata(false, AutoMovePropertyChangedCallback));
        public static readonly DependencyProperty AutoMoveHorizontalOffsetProperty = DependencyProperty.RegisterAttached("AutoMoveHorizontalOffset",typeof(double),typeof(PopupAutoMove),new FrameworkPropertyMetadata(16d));
        public static readonly DependencyProperty AutoMoveVerticalOffsetProperty =DependencyProperty.RegisterAttached("AutoMoveVerticalOffset",typeof(double),typeof(PopupAutoMove),new FrameworkPropertyMetadata(16d));
        public static readonly DependencyProperty AutoMoveRelativeToProperty = DependencyProperty.RegisterAttached("AutoMoveRelativeTo", typeof(FrameworkElement), typeof(PopupAutoMove));
        #endregion

        #region Get Set Helpers
        [AttachedPropertyBrowsableForType(typeof(Popup))]
        public static bool GetAutoMove(Popup element) => (bool)element.GetValue(AutoMoveProperty);
        public static void SetAutoMove(Popup element, bool value) => element.SetValue(AutoMoveProperty, value);

        [AttachedPropertyBrowsableForType(typeof(Popup))]
        public static double GetAutoMoveHorizontalOffset(Popup element) => (double)element.GetValue(AutoMoveHorizontalOffsetProperty);
        public static void SetAutoMoveHorizontalOffset(Popup element, double value) => element.SetValue(AutoMoveHorizontalOffsetProperty, value);

        [AttachedPropertyBrowsableForType(typeof(Popup))]
        public static double GetAutoMoveVerticalOffset(Popup element) => (double)element.GetValue(AutoMoveVerticalOffsetProperty);
        public static void SetAutoMoveVerticalOffset(Popup element, double value) => element.SetValue(AutoMoveVerticalOffsetProperty, value);

        [AttachedPropertyBrowsableForType(typeof(Popup))]
        public static FrameworkElement GetAutoMoveRelativeTo(Popup element) => (FrameworkElement)element.GetValue(AutoMoveRelativeToProperty);
        public static void SetAutoMoveRelativeTo(Popup element, FrameworkElement value) => element.SetValue(AutoMoveRelativeToProperty, value);
        #endregion

        #region Functions
        private static void AutoMovePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var Popup = (Popup)dependencyObject;
            if (eventArgs.OldValue != eventArgs.NewValue && eventArgs.NewValue != null)
            {
                var autoMove = (bool)eventArgs.NewValue;
                if (autoMove)
                {
                    Popup.Opened += Popup_Opened;
                    Popup.Closed += Popup_Closed;
                }
                else
                {
                    Popup.Opened -= Popup_Opened;
                    Popup.Closed -= Popup_Closed;
                }
            }
        }
        private static void Popup_Opened(object sender, EventArgs e)
        {
            var popup = (Popup)sender;

            //var RelativeTo = GetAutoMoveRelativeTo(popup);
            FrameworkElement target = popup.PlacementTarget as FrameworkElement;

            if (target != null)
            {
                // move the Popup on opening to the correct position
                MovePopup(target, popup);
                target.PreviewMouseMove += PopupTargetPreviewMouseMove;
                if (target is Slider sld)
                    sld.ValueChanged += Sld_ValueChanged;
            }
        }

        private static void Sld_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => PopupTargetPreviewMouseMove(sender, null);
        private static void Popup_Closed(object sender, EventArgs e)
        {
            var popup = (Popup)sender;

            var RelativeTo = GetAutoMoveRelativeTo(popup);
            FrameworkElement target = RelativeTo ?? popup.PlacementTarget as FrameworkElement;

            if (target != null)
            {
                target.PreviewMouseMove -= PopupTargetPreviewMouseMove;
                if (popup.PlacementTarget is Slider sld)
                    sld.ValueChanged -= Sld_ValueChanged;
            }
        }
        private static void PopupTargetPreviewMouseMove(object sender, MouseEventArgs e)
        {
            var target = sender as FrameworkElement;
            var popup = target?.GetChildOfType<Popup>();

            MovePopup(target, popup);
        }
        private static void MovePopup(IInputElement target, Popup popup)
        {
            if (popup == null || target == null) return;

            popup.Placement = PlacementMode.Relative;
            var hOffset = GetAutoMoveHorizontalOffset(popup);
            var vOffset = GetAutoMoveVerticalOffset(popup);


            FrameworkElement relativeTo = GetAutoMoveRelativeTo(popup) ?? popup.PlacementTarget as FrameworkElement;

            Point relativePoint = relativeTo.TranslatePoint(new Point(hOffset, vOffset), popup.PlacementTarget);
            if (hOffset == 0) // align horizontally on center
                relativePoint.X += (relativeTo.ActualWidth * .5) - ((popup.Child as FrameworkElement).ActualWidth * .5);
            if (vOffset == 0)
                relativePoint.Y += (relativeTo.ActualHeight * .5) - ((popup.Child as FrameworkElement).ActualHeight * .5);


            popup.HorizontalOffset = relativePoint.X;
            popup.VerticalOffset = relativePoint.Y;
        }
        #endregion
    }
}
