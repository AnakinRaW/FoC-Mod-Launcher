using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using AnakinRaW.CommonUtilities.Wpf.DPI;
using Vanara.PInvoke;

namespace AnakinRaW.CommonUtilities.Wpf.Utilities;

internal static class WindowHelper
{
    public static bool? ShowModal(Window window)
    {
        var dialogOwnerHandle = GetDialogOwnerHandle();
        return ShowModal(window, dialogOwnerHandle);
    }

    private static bool? ShowModal(Window window, IntPtr parent)
    {
        using (new WindowModalessScope(false))
        {
            using (DpiHelper.EnterDpiScope(DpiHelper.ProcessDpiAwarenessContext))
            {
                var helper = new WindowInteropHelper(window);
                helper.Owner = parent;
                if (window.WindowStartupLocation == WindowStartupLocation.CenterOwner)
                    window.SourceInitialized += (_, _) =>
                    {
                        if (!User32.GetWindowRect(parent, out var lpRect))
                            return;
                        var handle = helper.Handle;
                        if (HwndSource.FromHwnd(handle) == null)
                            return;
                        var deviceSize = handle.LogicalToDeviceSize(window.RenderSize);
                        var rect = CenterRectOnSingleMonitor(lpRect, (int)deviceSize.Width, (int)deviceSize.Height);
                        var logicalPoint = handle.DeviceToLogicalPoint(rect.GetPosition());
                        window.WindowStartupLocation = WindowStartupLocation.Manual;
                        window.Left = logicalPoint.X;
                        window.Top = logicalPoint.Y;
                    };
                return window.ShowDialog();
            }
        }
        
    }

    private static RECT CenterRectOnSingleMonitor(RECT parentRect, int childWidth, int childHeight)
    {
        FindMaximumSingleMonitorRectangle(parentRect, out var screenSubRect, out var monitorRect);
        return CenterInRect(screenSubRect, childWidth, childHeight, monitorRect);
    }

    private static RECT CenterInRect(RECT parentRect, int childWidth, int childHeight, RECT monitorClippingRect)
    {
        var rect = new RECT
        {
            Left = parentRect.Left + (parentRect.Width - childWidth) / 2,
            Top = parentRect.Top + (parentRect.Height - childHeight) / 2,
            Width = childWidth,
            Height = childHeight
        };
        rect.Left = Math.Min(rect.Right, monitorClippingRect.Right) - rect.Width;
        rect.Top = Math.Min(rect.Bottom, monitorClippingRect.Bottom) - rect.Height;
        rect.Left = Math.Max(rect.Left, monitorClippingRect.Left);
        rect.Top = Math.Max(rect.Top, monitorClippingRect.Top);
        return rect;
    }

    private static IntPtr GetDialogOwnerHandle()
    {
        return User32.GetActiveWindow().DangerousGetHandle();
    }

    internal static void FindMaximumSingleMonitorRectangle(RECT windowRect, out RECT screenSubRect, out RECT monitorRect)
    {
        var rects = new List<RECT>();
        User32.EnumDisplayMonitors(IntPtr.Zero, null, (hMonitor, _, _, _) =>
        {
            var monitorInfo = new User32.MONITORINFO
            {
                cbSize = (uint)Marshal.SizeOf(typeof(User32.MONITORINFO))
            };
            User32.GetMonitorInfo(hMonitor, ref monitorInfo);
            rects.Add(monitorInfo.rcWork);
            return true;
        }, IntPtr.Zero);
        long currentArea = 0;
        screenSubRect = new RECT
        {
            Left = 0,
            Right = 0,
            Top = 0,
            Bottom = 0
        };
        monitorRect = new RECT
        {
            Left = 0,
            Right = 0,
            Top = 0,
            Bottom = 0
        };
        foreach (var rect in rects)
        {
            var lprcSrc1 = rect;
            User32.IntersectRect(out var lprcDst, in lprcSrc1, in windowRect);
            var area = (long)(lprcDst.Width * lprcDst.Height);
            if (area > currentArea)
            {
                screenSubRect = lprcDst;
                monitorRect = rect;
                currentArea = area;
            }
        }
    }

    private class WindowModalessScope : IDisposable
    {
        private readonly bool _enable;

        private readonly HashSet<IntPtr> _windows = new();
        
        public WindowModalessScope(bool enable)
        {
            _enable = enable;
            EnableOrDisableApplicationWindows(enable);
        }


        public void Dispose()
        {
            foreach (var window in _windows)
            {
                var hwnd = HwndSource.FromHwnd(window);
                if (hwnd?.RootVisual is null)
                    continue;
                User32.EnableWindow(window, !_enable);
            }
        }

        private void EnableOrDisableApplicationWindows(bool enable)
        {
            if (Application.Current is null)
                return;
            foreach (var item in Application.Current.Windows)
            {
                if (item is not Window window)
                    continue;
                var handle = new WindowInteropHelper(window).Handle;
                var isEnabled = User32.IsWindowEnabled(handle);
                if (enable == isEnabled) 
                    continue;
                _windows.Add(handle);
                User32.EnableWindow(handle, enable);
            }
        }
    }
}