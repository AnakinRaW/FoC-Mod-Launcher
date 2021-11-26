using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace FocLauncher.Utilities
{
    internal static class ExtensionMethods
    {
        public static IEnumerable<T> FindDescendants<T>(this DependencyObject obj) where T : class
        {
            if (obj == null)
                return Enumerable.Empty<T>();
            List<T> descendants = new List<T>();
            obj.TraverseVisualTree<T>(child => descendants.Add(child));
            return descendants;
        }

        public static void TraverseVisualTree<T>(this DependencyObject obj, Action<T> action) where T : class
        {
            if (obj == null)
                return;
            for (var childIndex = 0; childIndex < VisualTreeHelper.GetChildrenCount(obj); ++childIndex)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, childIndex);
                if (child != null)
                {
                    T obj1 = child as T;
                    child.TraverseVisualTreeReverse(action);
                    if (obj1 != null)
                        action(obj1);
                }
            }
        }

        public static void TraverseVisualTreeReverse<T>(this DependencyObject obj, Action<T> action) where T : class
        {
            if (obj == null)
                return;
            for (var childIndex = VisualTreeHelper.GetChildrenCount(obj) - 1; childIndex >= 0; --childIndex)
            {
                var child = VisualTreeHelper.GetChild(obj, childIndex);
                if (child != null)
                {
                    T obj1 = child as T;
                    child.TraverseVisualTreeReverse(action);
                    if (obj1 != null)
                        action(obj1);
                }
            }
        }

        public static bool IsConnectedToPresentationSource(this DependencyObject obj)
        {
            return PresentationSource.FromDependencyObject(obj) != null;
        }
    }
}