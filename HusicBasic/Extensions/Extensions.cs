using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using HusicBasic.Models;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using TNO.BitUtilities;

namespace HusicBasic
{
    public static class Extensions
    {
        #region Functions
        public static string FormatTime(this TimeSpan time, bool addMinus = false)
        {
            int minutes = ((time.Days * 24) + time.Hours) * 60 + time.Minutes;
            return $"{(addMinus ? "-" : "")}{minutes.ToString().PadLeft(2, '0')}:{time.Seconds.ToString().PadLeft(2, '0')}";
        }
        public static string FormatSimple(this TimeSpan time, bool includeMilliseconds = false)
        {
            if (time.Days > 0) return $"{time.Days}d";
            if (time.Hours > 0) return $"{time.Hours}h";
            if (time.Minutes > 0) return $"{time.Minutes}m";
            if (time.Seconds > 0) return $"{time.Seconds}s";
            if (includeMilliseconds) return $"{time.Milliseconds}ms";
            return "0s";
        }
        public static string FormatNice(this TimeSpan time)
        {
            List<string> parts = new List<string>();
            if (time.Days > 0 || time.Hours > 0)
                parts.Add($"{(time.Days * 24) + time.Hours:n0}h");
            if (time.Minutes > 0) parts.Add($"{time.Minutes}m");
            if (time.Seconds > 0) parts.Add($"{time.Seconds}s");

            if (parts.Count == 0) parts.Add("0s");

            return string.Join(' ', parts);
        }
        public static string FormatNice(this DateTime date)
        {
            string daySuffix;
            int daySuffixNum = date.Day % 10;
            if (daySuffixNum == 1) daySuffix = "st";
            else if (daySuffixNum == 2) daySuffix = "nd";
            else if (daySuffixNum == 3) daySuffix = "rd";
            else daySuffix = "th";

            return $"{date.Day}{daySuffix} {date:MMMM} {date.Year}";
        }
        public static T ExcludeOutliers<T>(this T values) where T : IList<double>, new()
        {
            T n = new T();
            double sd = values.GetStandardDeviation(out double mean);
            foreach(double val in values)
            {
                if ((val >= mean - sd) && (val <= mean + sd))
                    n.Insert(n.Count, val);
            }
            return n;
        }
        public static double GetCorrectedAverage(this List<double> values)
        {
            List<double> corrected = values.ExcludeOutliers();
            return corrected.GetAverage();
        }
        public static double GetStandardDeviation(this IEnumerable<double> values, out double mean)
        {
            double sum = 0;
            int count = 0;
            mean = values.GetAverage();
            foreach (double val in values)
            {
                sum += Math.Pow(val - mean, 2);
                count++;
            }

            return sum / count;
        }
        public static double GetAverage(this IEnumerable<double> values)
        {
            double total = 0;
            double c = 0;
            foreach (double val in values)
            {
                total += val;
                c++;
            }
            return total / c;
        }
        #endregion

        #region Logical Node Traversal
        public static ICollection<T> GetChildrenOfType<T>(this DependencyObject obj) where T : DependencyObject
        {
            Collection<T> collection = new Collection<T>();
            obj?.GetChildrenOfType(collection);
            return collection;
        }
        private static void GetChildrenOfType<T>(this DependencyObject obj, ICollection<T> collection) where T : DependencyObject
        {
            if (obj == null) return;
            int childCount = VisualTreeHelper.GetChildrenCount(obj);

            for(int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T childT)
                    collection.Add(childT);
                child.GetChildrenOfType(collection);
            }
        }
        public static T GetChildOfType<T>(this DependencyObject obj) where T : DependencyObject
        {
            if (obj == null) return null;

            int childCount = VisualTreeHelper.GetChildrenCount(obj);

            // Breadth first
            DependencyObject[] children = new DependencyObject[childCount];
            for(int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T childT) return childT;
                children[i] = child;
            }

            foreach(DependencyObject child in children)
            {
                T res = GetChildOfType<T>(child);
                if (res != null) return res;
            }
            return null;
        }
        public static void Sort<T>(this ObservableCollection<T> collection) where T : IComparable<T>, IEquatable<T>
        {
            List<T> sorted = collection.OrderBy(x => x).ToList();

            for(int i = 0; i < sorted.Count;i++)
            {
                if (!collection[i].Equals(sorted[i]))
                {
                    int i2 = Search(collection, i + 1, sorted[i]);
                    collection.Move(i2, i);
                }
            }
        }
        public static int Search<T>(ObservableCollection<T> collection, int startIndex, T item)
        {
            for (int i = startIndex; i < collection.Count; i++)
                if (item.Equals(collection[i]))
                    return i;
            return -1;
        }
        #endregion

        #region Dialogs
        public static void Show(this IDialogService service, string name, Action<IDialogResult> callback, string window) => service.Show(name, new DialogParameters(), callback, window);
        public static void ShowConfirmation(this IDialogService service, Action<IDialogResult> callback, string message)
        {
            DialogParameters param = new DialogParameters();
            param.Add("message", message);
            service.Show("Confirmation", param, callback, "Popup");
        }
        public static void ConfirmDeletion(this IDialogService service, Action<IDialogResult> callback, PlaylistModel playlist)
            => ConfirmDeletion(service, callback, "playlist", playlist.Title);
        public static void ConfirmDeletion(this IDialogService service, Action<IDialogResult> callback, SongModel song)
        {
            string extra = string.IsNullOrWhiteSpace(song.YoutubeID) ? null : "This will also remove the source file.";
            ConfirmDeletion(service, callback, "song", song.Title, extra);
        }
        public static void ConfirmDeletion(this IDialogService service, Action<IDialogResult> callback, string type, string name)
            => ConfirmDeletion(service, callback, type, name, null);
        public static void ConfirmDeletion(this IDialogService service, Action<IDialogResult> callback, string type, string name, string extra)
        {
            DialogParameters param = new DialogParameters
            {
                { "type", type },
                { "name", name },
            };
            if (!string.IsNullOrWhiteSpace(extra))
                param.Add("extra", extra);
            service.Show("ConfirmDeletion", param, callback, "Popup");
        }
        #endregion
    }
}
