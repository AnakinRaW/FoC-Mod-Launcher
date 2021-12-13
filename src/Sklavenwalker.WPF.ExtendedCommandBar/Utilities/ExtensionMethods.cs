using System;
using System.Windows;
using System.Windows.Interop;
using Sklavenwalker.Wpf.CommandBar.NativeMethods;

namespace Sklavenwalker.Wpf.CommandBar.Utilities;

internal static class ExtensionMethods
{
    public static bool AcquireWin32Focus(this DependencyObject obj, out IntPtr previousFocus)
    {
        if (PresentationSource.FromDependencyObject(obj) is HwndSource hwndSource)
        {
            previousFocus = User32.GetFocus();
            if (previousFocus != hwndSource.Handle)
            {
                User32.SetFocus(hwndSource.Handle);
                return true;
            }
        }

        previousFocus = IntPtr.Zero;
        return false;
    }
}