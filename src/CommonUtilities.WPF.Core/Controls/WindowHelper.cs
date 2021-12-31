using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using Sklavenwalker.CommonUtilities.Wpf.DPI;
using Sklavenwalker.CommonUtilities.Wpf.NativeMethods;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

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
                        var logicalPoint = handle.DeviceToLogicalPoint(rect.Position);
                        window.WindowStartupLocation = WindowStartupLocation.Manual;
                        window.Left = logicalPoint.X;
                        window.Top = logicalPoint.Y;
                    };
                return window.ShowDialog();
            }
        }
        
    }

    private static RectStruct CenterRectOnSingleMonitor(RectStruct parentRect, int childWidth, int childHeight)
    {
        User32.FindMaximumSingleMonitorRectangle(parentRect, out var screenSubRect, out var monitorRect);
        return CenterInRect(screenSubRect, childWidth, childHeight, monitorRect);
    }

    private static RectStruct CenterInRect(RectStruct parentRect, int childWidth, int childHeight, RectStruct monitorClippingRect)
    {
        var rect = new RectStruct
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
        return User32.GetActiveWindow();
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