using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using FocLauncher.NativeMethods;

namespace FocLauncher.ScreenUtilities
{
    public static class DpiHelper
    {
        private static readonly Lazy<bool> IsPerMonitorAwarenessEnabledLazy = new Lazy<bool>(IsProcessPreMonitorDpiAware);
        private static readonly Lazy<Dpi> SystemDpiLazy = new Lazy<Dpi>(GetSystemDpi);
        private static readonly Lazy<DpiScale> SystemDpiScaleLazy = new Lazy<DpiScale>(() =>
        {
            var systemDpi = SystemDpi;
            return new DpiScale(systemDpi.X / 96.0, systemDpi.Y / 96.0);
        });

        public static bool IsPerMonitorAwarenessEnabled => IsPerMonitorAwarenessEnabledLazy.Value;
        public static Dpi SystemDpi => SystemDpiLazy.Value;
        public static DpiScale SystemDpiScale => SystemDpiScaleLazy.Value;

        public static Dpi GetMonitorDpi(this IntPtr monitorHandle)
        {
            var dpi = IsPerMonitorAwarenessEnabled ? GetMonitorDpiCore(monitorHandle) : SystemDpi;
            if (!dpi.IsValid)
                throw new DpiErrorException(dpi, $"The dpi '{dpi}' is invalid!");
            return dpi;
        }

        internal static Dpi GetMonitorDpiCore(IntPtr monitorHandle)
        {
            return ShCore.GetDpiForMonitor(monitorHandle, MonitorDpiType.MdtEffectiveDpi, out var x, out var y) != 0
                ? Dpi.Default
                : new Dpi(x, y);
        }

        public static int DeviceToLogicalUnitsY(this Visual visual, int value)
        {
            return visual.DeviceToLogicalUnitsY<int>(value);
        }

        public static int DeviceToLogicalUnitsX(this Visual visual, int value)
        {
            return visual.DeviceToLogicalUnitsX<int>(value);
        }

        public static double DeviceToLogicalUnitsX(this Visual visual, double value)
        {
            return visual.DeviceToLogicalUnitsX<double>(value);
        }

        public static double DeviceToLogicalUnitsY(this Visual visual, double value)
        {
            return visual.DeviceToLogicalUnitsY<double>(value);
        }

        private static T DeviceToLogicalUnitsX<T>(this Visual visual, T value) where T : IConvertible
        {
            return visual.DeviceToLogicalUnits(value, true);
        }

        private static T DeviceToLogicalUnitsY<T>(this Visual visual, T value) where T : IConvertible
        {
            return visual.DeviceToLogicalUnits(value, false);
        }

        private static T DeviceToLogicalUnits<T>(this Visual visual, T value, bool getX) where T : IConvertible
        {
            var dpiScale = GetDpiScale(visual);
            var scaleValue = getX ? dpiScale.DpiScaleX : dpiScale.DpiScaleY;
            return (T) Convert.ChangeType(value.ToDouble(null) / scaleValue, typeof(T));
        }

        private static bool IsProcessPreMonitorDpiAware()
        {
            if (!(Environment.OSVersion.Platform == PlatformID.Win32NT && 
                  (Environment.OSVersion.Version.Major > 6 || Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor >= 2)))
                return false;

            try
            {
                ShCore.GetProcessDpiAwareness(Process.GetCurrentProcess().Handle, out var awareness);

                switch (awareness)
                {
                    case ProcessDpiAwareness.ProcessDpiUnaware:
                    case ProcessDpiAwareness.ProcessSystemDpiAware:
                        return false;
                    case ProcessDpiAwareness.ProcessPreMonitorDpiAware:
                        return true;
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private static DpiScale GetDpiScale(Visual visual)
        {
            if (visual == null)
                throw new ArgumentNullException(nameof(visual));
            var dpiScale = IsPerMonitorAwarenessEnabled ? VisualTreeHelper.GetDpi(visual) : SystemDpiScale;
            if (!dpiScale.IsValid())
                throw new InvalidOperationException($"The dpi scale '{dpiScale}' is invalid!");
            return dpiScale;
        }

        private static Dpi GetSystemDpi()
        {
            var dc = User32.GetDC(IntPtr.Zero);
            if (dc == IntPtr.Zero) 
                return Dpi.Default;
            try
            {
                return new Dpi(Gdi32.GetDeviceCaps(dc, Gdi32.LogPixelsX), Gdi32.GetDeviceCaps(dc, Gdi32.LogPixelsY));
            }
            finally
            {
                User32.ReleaseDC(IntPtr.Zero, dc);
            }
        }

        private static bool IsValid(this DpiScale dpiScale)
        {
            return new Dpi(dpiScale.DpiScaleX, dpiScale.DpiScaleX).IsValid;
        }
    }
}