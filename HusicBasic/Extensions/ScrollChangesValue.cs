using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HusicBasic
{
    public static class ScrollChangesValue
    {
        #region Mouse Scroll for Slider
        public static readonly DependencyProperty ScrollChangesValueProperty = DependencyProperty.RegisterAttached("ScrollChangesValue", typeof(bool), typeof(ScrollChangesValue), new PropertyMetadata(false, ScrollChangesValuePropertyChanged));
        public static readonly DependencyProperty ScrollChangesValueSpeedProperty = DependencyProperty.RegisterAttached("ScrollChangesValueSpeed", typeof(double), typeof(ScrollChangesValue), new PropertyMetadata(10d));
        public static readonly DependencyProperty ScrollChangesValueCallbackProperty = DependencyProperty.RegisterAttached("ScrollChangesValueCallback", typeof(ICommand), typeof(ScrollChangesValue), new PropertyMetadata(null));

        [AttachedPropertyBrowsableForType(typeof(Slider))]
        public static bool GetScrollChangesValue(Slider element) => (bool)element.GetValue(ScrollChangesValueProperty);
        public static void SetScrollChangesValue(Slider element, bool value) => element.SetValue(ScrollChangesValueProperty, value);

        [AttachedPropertyBrowsableForType(typeof(Slider))]
        public static double GetScrollChangesValueSpeed(Slider element) => (double)element.GetValue(ScrollChangesValueSpeedProperty);
        public static void SetScrollChangesValueSpeed(Slider element, double value) => element.SetValue(ScrollChangesValueSpeedProperty, value);

        [AttachedPropertyBrowsableForType(typeof(Slider))]
        public static ICommand GetScrollChangesValueCallback(Slider element) => (ICommand)element.GetValue(ScrollChangesValueCallbackProperty);
        public static void SetScrollChangesValueCallback(Slider element, ICommand value) => element.SetValue(ScrollChangesValueCallbackProperty, value);

        private static void ScrollChangesValuePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (!(dependencyObject is Slider sld)) return;
            if (eventArgs.OldValue == eventArgs.NewValue || eventArgs.NewValue == null) return;

            if (((bool)eventArgs.NewValue) == true)
                sld.MouseWheel += ScrollChangesValue_MouseWheel;
            else
                sld.MouseWheel -= ScrollChangesValue_MouseWheel;
        }
        private static void ScrollChangesValue_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!(sender is Slider sld)) return;
            double newVal = Math.Clamp(sld.Value + (Math.Sign(e.Delta) * GetScrollChangesValueSpeed(sld)), sld.Minimum, sld.Maximum);
            ICommand callback = GetScrollChangesValueCallback(sld);

            if (callback != null && callback.CanExecute(newVal))
                callback.Execute(newVal);
        }
        #endregion
    }
}
