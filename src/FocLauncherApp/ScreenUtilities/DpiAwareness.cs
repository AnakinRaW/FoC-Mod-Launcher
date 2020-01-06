using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Media;

namespace FocLauncherApp.ScreenUtilities
{
    public static class DpiAwareness
    {
        private static bool? _isPerMonitorAwarenessEnabled;
        private static DpiAwarenessContext? _processDpiAwarenessContext;

        private static readonly Lazy<double> LazySystemDpiX = new Lazy<double>(() => GetSystemDpi(true));
        private static readonly Lazy<double> LazySystemDpiY = new Lazy<double>(() => GetSystemDpi(false));

        private static readonly Type VisualTreeHelperType = typeof(VisualTreeHelper);
        private static readonly Lazy<MethodInfo> LazyGetDpiMethod = new Lazy<MethodInfo>(() => VisualTreeHelperType.GetMethod("GetDpi", BindingFlags.Static | BindingFlags.Public));

        private static readonly Lazy<PropertyInfo> LazyDpiScaleXProperty = new Lazy<PropertyInfo>(() => GetDpiScaleProperty("DpiScaleX"));
        private static readonly Lazy<PropertyInfo> LazyDpiScaleYProperty = new Lazy<PropertyInfo>(() => GetDpiScaleProperty("DpiScaleY"));

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
                    var awarenessContext = DpiAwarenessContext.Unaware;
                    try
                    {
                        NativeMethods.NativeMethods.GetProcessDpiAwareness(Process.GetCurrentProcess().Handle, out ProcessDpiAwareness awareness);
                        switch (awareness)
                        {
                            case ProcessDpiAwareness.ProcessSystemDpiAware:
                                awarenessContext = DpiAwarenessContext.SystemAware;
                                break;
                            case ProcessDpiAwareness.ProcessPerMonitorDpiAware:
                                awarenessContext = DpiAwarenessContext.PerMonitorAwareV2;
                                break;
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        _processDpiAwarenessContext = awarenessContext;
                    }
                }

                return _processDpiAwarenessContext.Value;
            }
        }

        public static void GetMonitorDpi(this IntPtr hmonitor, out double dpiX, out double dpiY)
        {
            ValidateIntPtr(hmonitor);
            Dpi dpi = new Dpi();
            if (IsPerMonitorAwarenessEnabled)
            {
                dpi = (Dpi)GetDpiForMonitorFunc(hmonitor);
                dpiX = dpi.X;
                dpiY = dpi.Y;
            }
            else
            {
                dpiX = LazySystemDpiX.Value;
                dpiY = LazySystemDpiY.Value;
            }
            if (!IsValidDpi(dpiX))
                throw new MonitorDpiAwarenessException(hmonitor, dpi.HResult, dpiX, "DPI is invalid");
            if (!IsValidDpi(dpiY))
                throw new MonitorDpiAwarenessException(hmonitor, dpi.HResult, dpiY, "DPI is invalid");
        }

        public static int DeviceToLogicalUnitsY(this Visual visual, int value)
        {
            return visual.DeviceToLogicalUnitsY<int>(value);
        }

        public static int DeviceToLogicalUnitsX(this Visual visual, int value)
        {
            return visual.DeviceToLogicalUnitsX<int>(value);
        }

        private static T DeviceToLogicalUnitsX<T>(this Visual visual, T value) where T : IConvertible
        {
            return visual.DeviceToLogicalUnits(value, true);
        }

        private static T DeviceToLogicalUnitsY<T>(this Visual visual, T value) where T : IConvertible
        {
            return visual.DeviceToLogicalUnits(value, false);
        }

        private static T DeviceToLogicalUnits<T>(this Visual visual, T value, bool getDpiScaleX) where T : IConvertible
        {
            var dpiScale = GetDpiScale(visual, getDpiScaleX);
            return (T)Convert.ChangeType(value.ToDouble(null) / dpiScale, typeof(T));
        }

        private static void ValidateIntPtr(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                throw new ArgumentException("Ptr must not be zero");
        }

        private static double GetDpiScale(Visual visual, bool getDpiScaleX)
        {
            if (visual == null)
                throw new ArgumentNullException(nameof(visual));
            double num;
            if (IsPerMonitorAwarenessEnabled)
            {
                var dpiScaleObject = GetDpiScaleObject(visual);
                num = GetDpiScalePropertyValue(getDpiScaleX ? LazyDpiScaleXProperty.Value : LazyDpiScaleYProperty.Value, dpiScaleObject);
            }
            else
                num = getDpiScaleX ? LazySystemDpiScaleX.Value : LazySystemDpiScaleY.Value;

            if (!IsValidDpi(num))
                throw new VisualDpiAwarenessException(visual, num, "Invalid DPI");
            return num;

        }

        private static bool IsValidDpi(double dpi)
        {
            return !double.IsInfinity(dpi) && !double.IsNaN(dpi) && dpi > 0.0;
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
                    num = NativeMethods.NativeMethods.GetDeviceCaps(dc, (int)index);
                }
                finally
                {
                    NativeMethods.NativeMethods.ReleaseDC(IntPtr.Zero, dc);
                }
            }

            return num;
        }

        private static PropertyInfo GetDpiScaleProperty(string propertyName)
        {
            var methodInfo = LazyGetDpiMethod.Value;
            var type = methodInfo != null ? methodInfo.ReturnType : null;
            return type == null ? null : type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        }

        private static double GetDpiScalePropertyValue(PropertyInfo dpiScaleProperty, object dpiScaleObject)
        {
            if (dpiScaleProperty == null)
                throw new ArgumentNullException(nameof(dpiScaleProperty));
            if (dpiScaleObject == null)
                throw new ArgumentNullException(nameof(dpiScaleObject));
            var obj = dpiScaleProperty.GetValue(dpiScaleObject);
            if (obj == null)
                throw new InvalidOperationException("Error getting DPI scale");
            return Convert.ToDouble(obj);
        }

        private static object GetDpiScaleObject(Visual visual)
        {
            var methodInfo = LazyGetDpiMethod.Value;
            if (methodInfo == null)
                return null;
            return methodInfo.Invoke(null, new object[] { visual });
        }

        private static object GetDpiForMonitor(IntPtr hmonitor)
        {
            Dpi dpi = new Dpi
            {
                HResult = NativeMethods.NativeMethods.GetDpiForMonitor(hmonitor, MonitorDpiType.MdtEffectiveDpi, out var dpiX,
                    out var dpiY)
            };
            if (dpi.HResult == 0)
            {
                dpi.X = dpiX;
                dpi.Y = dpiY;
            }
            return dpi;
        }

        private enum DeviceCaps
        {
            LogPixlelsX = 88,
            LogPixelsY = 90,
        }

        internal enum ProcessDpiAwareness
        {
            ProcessDpiUnaware,
            ProcessSystemDpiAware,
            ProcessPerMonitorDpiAware,
        }

        internal enum MonitorDpiType
        {
            MdtEffectiveDpi,
            MDT_ANGULAR_DPI,
            MDT_RAW_DPI,
            MDT_DEFAULT,
        }

        private struct Dpi
        {
            public double X;
            public double Y;
            public int HResult;

            public Dpi(double x, double y, int hr = 0)
            {
                X = x;
                Y = y;
                HResult = hr;
            }
        }
    }
}