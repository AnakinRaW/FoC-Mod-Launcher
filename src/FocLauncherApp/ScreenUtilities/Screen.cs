using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using FocLauncherApp.NativeMethods;

namespace FocLauncherApp.ScreenUtilities
{
    internal static class Screen
    {
        private static bool? _isPerMonitorAwarenessEnabled;
        private static DpiAwarenessContext? _processDpiAwarenessContext;

        private static readonly Lazy<double> LazySystemDpiX = new Lazy<double>(() => GetSystemDpi(true));
        private static readonly Lazy<double> LazySystemDpiY = new Lazy<double>(() => GetSystemDpi(false));
        private static readonly Lazy<double> LazySystemDpiScaleX = new Lazy<double>(() => LazySystemDpiX.Value / 96.0);
        private static readonly Lazy<double> LazySystemDpiScaleY = new Lazy<double>(() => LazySystemDpiY.Value / 96.0);
        private static readonly Func<IntPtr, object> GetDpiForMonitorFunc = GetDpiForMonitor;

        public static bool IsPerMonitorAwarenessEnabled
        {
            get
            {
                if (!_isPerMonitorAwarenessEnabled.HasValue)
                    _isPerMonitorAwarenessEnabled = ProcessDpiAwarenessContext == DpiAwarenessContext.PerMonitorAwareV2;
                return _isPerMonitorAwarenessEnabled.Value;
            }
        }

        public static DpiAwarenessContext ProcessDpiAwarenessContext
        {
            get
            {
                if (!_processDpiAwarenessContext.HasValue)
                {
                    var awarenessContext1 = DpiAwarenessContext.Unaware;
                    try
                    {
                        NativeMethods.NativeMethods.GetProcessDpiAwareness(Process.GetCurrentProcess().Handle, out var awareness);
                        switch (awareness)
                        {
                            case ProcessDpiAwareness.ProcessSystemDpiAware:
                                awarenessContext1 = DpiAwarenessContext.SystemAware;
                                break;
                            case ProcessDpiAwareness.ProcessPerMonitorDpiAware:
                                try
                                {
                                    var awarenessContext2 = NativeMethods.NativeMethods.GetThreadDpiAwarenessContext();
                                    if (NativeMethods.NativeMethods.AreDpiAwarenessContextsEqual(awarenessContext2, new IntPtr(-4)))
                                    {
                                        awarenessContext1 = DpiAwarenessContext.PerMonitorAwareV2;
                                        break;
                                    }
                                    if (NativeMethods.NativeMethods.AreDpiAwarenessContextsEqual(awarenessContext2, new IntPtr(-3)))
                                        awarenessContext1 = DpiAwarenessContext.PerMonitorAware;
                                    break;
                                }
                                catch
                                {
                                    awarenessContext1 = DpiAwarenessContext.PerMonitorAware;
                                    break;
                                }
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        _processDpiAwarenessContext = awarenessContext1;
                    }
                }
                return _processDpiAwarenessContext.Value;
            }
        }

        public static double LogicalToDeviceUnitsX(IntPtr windowHandle, double value)
        {
            GetMonitorDpi(windowHandle, out _, out var dpiY);
            return ScaleLogicalToDevice(dpiY, value);
        }

        public static double LogicalToDeviceUnitsY(IntPtr windowHandle, double value)
        {
            GetMonitorDpi(windowHandle, out _, out var dpiY);
            return ScaleLogicalToDevice(dpiY, value);
        }

        public static void SetInitialWindowRect(IntPtr hwnd, Window window, Int32Rect windowBounds)
        {
            if (hwnd == IntPtr.Zero)
                throw new ArgumentException(nameof(hwnd));
            SetWindowDpi(hwnd, window, windowBounds);
            window.Top = window.DeviceToLogicalUnitsY(windowBounds.Y);
            window.Left = window.DeviceToLogicalUnitsX(windowBounds.X);
            window.Width = window.DeviceToLogicalUnitsX(windowBounds.Width);
            window.Height = window.DeviceToLogicalUnitsY(windowBounds.Height);
        }

        public static int DeviceToLogicalUnitsX(this Visual visual, int value)
        {
            return visual.DeviceToLogicalUnitsX<int>(value);
        }

        private static T DeviceToLogicalUnitsX<T>(this Visual visual, T value) where T : IConvertible
        {
            return visual.DeviceToLogicalUnits<T>(value, true);
        }

        public static int DeviceToLogicalUnitsY(this Visual visual, int value)
        {
            return visual.DeviceToLogicalUnitsY<int>(value);
        }

        private static T DeviceToLogicalUnitsY<T>(this Visual visual, T value) where T : IConvertible
        {
            return visual.DeviceToLogicalUnits<T>(value, false);
        }

        private static T DeviceToLogicalUnits<T>(this Visual visual, T value, bool getDpiScaleX) where T : IConvertible
        {
            var dpiScale = GetDpiScale(visual, getDpiScaleX);
            return (T)Convert.ChangeType((value.ToDouble(null) / dpiScale), typeof(T));
        }

        private static double ScaleLogicalToDevice(double dpi, double value)
        {
            return value * dpi / 96.0;
        }

        private static double GetDpiScale(Visual visual, bool getDpiScaleX)
        {
            double num;
            if (IsPerMonitorAwarenessEnabled)
            {
                var dpiScale = VisualTreeHelper.GetDpi(visual);
                num = getDpiScaleX ? dpiScale.DpiScaleX : dpiScale.DpiScaleY;
            }
            else
                num = getDpiScaleX ? LazySystemDpiScaleX.Value : LazySystemDpiScaleY.Value;

            if (!IsValidDpi(num))
                throw new InvalidOperationException();
            return num;
        }

        private static void SetWindowDpi(IntPtr hwnd, Window window, Int32Rect windowBounds)
        {
            var rect = RECT.FromInt32Rect(windowBounds);
            var monitorDpiScale = GetMonitorDpiScale(hwnd);
            var wParam = new IntPtr((int)monitorDpiScale.PixelsPerInchX | (int)monitorDpiScale.PixelsPerInchY << 16);
            var num = Marshal.AllocCoTaskMem(Marshal.SizeOf(rect));
            try
            {
                Marshal.StructureToPtr(rect, num, false);
                NativeMethods.NativeMethods.SendMessage(hwnd, 736, wParam, num);
                VisualTreeHelper.SetRootDpi(window, monitorDpiScale);
            }
            finally
            {
                Marshal.FreeCoTaskMem(num);
            }
        }

        private static DpiScale GetMonitorDpiScale(IntPtr windowHandle)
        {
            return new DpiScale(LogicalToDeviceUnitsX(windowHandle, 1.0), LogicalToDeviceUnitsY(windowHandle, 1.0));
        }

        private static void GetMonitorDpi(IntPtr windowHandle, out double dpiX, out double dpiY)
        {
            var monitor = NativeMethods.NativeMethods.MonitorFromWindow(windowHandle, MonitorOpts.DefaultToNearest);

            var m = new MonitorInfo
            {
                CbSize = (uint) Marshal.SizeOf(typeof(MonitorInfo))
            };
            NativeMethods.NativeMethods.GetMonitorInfo(monitor, ref m);

            if (IsPerMonitorAwarenessEnabled)
            {
                var dpi = (Dpi) GetDpiForMonitorFunc(monitor);
                dpiX = dpi.X;
                dpiY = dpi.Y;
            }
            else
            {
                dpiX = LazySystemDpiX.Value;
                dpiY = LazySystemDpiY.Value;
            }
            if (!IsValidDpi(dpiX))
                throw new InvalidOperationException();
            if (!IsValidDpi(dpiY))
                throw new InvalidOperationException();
        }

        private static object GetDpiForMonitor(IntPtr hmonitor)
        {
            var dpi = new Dpi
            {
                HResult = NativeMethods.NativeMethods.GetDpiForMonitor(hmonitor, MonitorDpiType.MdtEffectiveDpi, out var dpiX, out var dpiY)
            };
            if (dpi.HResult == 0)
            {
                dpi.X = dpiX;
                dpi.Y = dpiY;
            }
            return dpi;
        }


        private static double GetSystemDpi(bool getDpiX)
        {
            var dc = NativeMethods.NativeMethods.GetDC(IntPtr.Zero);
            var num = 96.0;
            if (dc != IntPtr.Zero)
            {
                try
                {
                    var index = getDpiX ? DeviceCaps.LogPixlelsX : DeviceCaps.LogPixelsY;
                    num = NativeMethods.NativeMethods.GetDeviceCaps(dc, index);
                }
                finally
                {
                    NativeMethods.NativeMethods.ReleaseDC(IntPtr.Zero, dc);
                }
            }
            return num;
        }
        

        private static bool IsValidDpi(double dpi)
        {
            return !double.IsInfinity(dpi) && !double.IsNaN(dpi) && dpi > 0.0;
        }

        private struct Dpi
        {
            public double X;
            public double Y;
            public int HResult;
        }
    }
}