using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using FocLauncher.NativeMethods;

namespace FocLauncher.Utilities
{
    internal static class FocusHelper
    {
        public static void MoveFocusInto(UIElement element)
        {
            if (IsKeyboardFocusWithin(element))
                return;
            var request = new TraversalRequest(FocusNavigationDirection.First);
            if (element.MoveFocus(request) || IsKeyboardFocusWithin(element))
                return;
            element.Dispatcher.BeginInvoke(DispatcherPriority.Render, (Action)(() =>
            {
                element.MoveFocus(request);
            }));
        }

        public static bool IsKeyboardFocusWithin(IntPtr hwnd)
        {
            return IsChildOrSame(hwnd, User32.GetFocus());
        }

        public static bool IsKeyboardFocusWithin(UIElement element)
        {
            if (element.IsKeyboardFocusWithin)
                return true;
            var focus = User32.GetFocus();
            if (element.FindDescendants<HwndHost>().Any(descendant => IsChildOrSame(descendant.Handle, focus)))
                return true;
            return element is HwndHost hwndHost && IsKeyboardFocusWithin(hwndHost.Handle);
        }
        
        private static bool IsChildOrSame(IntPtr hwndParent, IntPtr hwndChild) => hwndParent == hwndChild || User32.IsChild(hwndParent, hwndChild);
    }
}