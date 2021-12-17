using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Sklavenwalker.CommonUtilities.Wpf.NativeMethods;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.DPI;

public static class DpiAwareness
{
    private static readonly Lazy<double> LazySystemDpiX = new(() => GetSystemDpi(true));
    private static readonly Lazy<double> LazySystemDpiY = new(() => GetSystemDpi(false));
    private static readonly Lazy<double> LazySystemDpiScaleX = new(() => LazySystemDpiX.Value / 96.0);
    private static readonly Lazy<double> LazySystemDpiScaleY = new(() => LazySystemDpiY.Value / 96.0);
    private static bool? _isPerMonitorAwarenessEnabled;
    private static DpiAwarenessContext? _processDpiAwarenessContext;
    private static BitmapScalingMode _bitmapScalingMode;

    public static bool IsPerMonitorAwarenessEnabled
    {
        get
        {
            _isPerMonitorAwarenessEnabled ??= ProcessDpiAwarenessContext == DpiAwarenessContext.PerMonitorAwareV2;
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
                    ShCore.GetProcessDpiAwareness(Process.GetCurrentProcess().Handle, out var awareness);
                    switch (awareness)
                    {
                        case ProcessDpiAwareness.ProcessSystemDpiAware:
                            awarenessContext = DpiAwarenessContext.SystemAware;
                            break;
                        case ProcessDpiAwareness.ProcessPerMonitorDpiAware:
                            try
                            {
                                var threadContext = User32.GetThreadDpiAwarenessContext();
                                if (User32.AreDpiAwarenessContextsEqual(threadContext, new IntPtr(-4)))
                                {
                                    awarenessContext = DpiAwarenessContext.PerMonitorAwareV2;
                                    break;
                                }
                                if (User32.AreDpiAwarenessContextsEqual(threadContext, new IntPtr(-3)))
                                {
                                    awarenessContext = DpiAwarenessContext.PerMonitorAware;
                                }
                                break;
                            }
                            catch
                            {
                                awarenessContext = DpiAwarenessContext.PerMonitorAware;
                                break;
                            }
                    }
                }
                catch
                {
                    // Ignore
                }
                finally
                {
                    _processDpiAwarenessContext = awarenessContext;
                }
            }
            return _processDpiAwarenessContext.Value;
        }
    }

    public static BitmapScalingMode BitmapScalingMode
    {
        get
        {
            if (_bitmapScalingMode == BitmapScalingMode.Unspecified)
            {
                var dpiScalePercentX = (int)LazySystemDpiScaleX.Value * 100;
                _bitmapScalingMode = GetDefaultBitmapScalingMode(dpiScalePercentX);
            }
            return _bitmapScalingMode;
        }
    }

    public static double SystemDpiX => LazySystemDpiX.Value;

    public static double SystemDpiY => LazySystemDpiY.Value;

    public static double SystemDpiXScale => LazySystemDpiScaleX.Value;

    public static double SystemDpiYScale => LazySystemDpiScaleY.Value;

    private static BitmapScalingMode GetDefaultBitmapScalingMode(int dpiScalePercent)
    {
        if (dpiScalePercent % 100 == 0)
            return BitmapScalingMode.NearestNeighbor;
        return dpiScalePercent < 100 ? BitmapScalingMode.LowQuality : BitmapScalingMode.HighQuality;
    }

    private static bool IsValidDpi(double dpi) => !double.IsInfinity(dpi) && !double.IsNaN(dpi) && dpi > 0.0;

    private static double GetSystemDpi(bool getDpiX)
    {
        var dc = User32.GetDC(IntPtr.Zero);
        var systemDpi = 96.0;
        if (dc != IntPtr.Zero)
        {
            try
            {
                var index = getDpiX ? DeviceCaps.LogPixelsX : DeviceCaps.LogPixelsY;
                systemDpi = Gdi32.GetDeviceCaps(dc, (int)index);
            }
            finally
            {
                User32.ReleaseDC(IntPtr.Zero, dc);
            }
        }
        return systemDpi;
    }

    public static double GetDpiX(this Visual visual) => GetDpi(visual, true);

    public static double GetDpiXScale(this Visual visual) => GetDpiScale(visual, true);

    public static double GetDpiY(this Visual visual) => GetDpi(visual, false);

    public static double GetDpiYScale(this Visual visual) => GetDpiScale(visual, false);

    public static void HookDpiChanged(this HwndHost hwndHost, RoutedEventHandler callback) => HookDpiChanged<HwndHost>(hwndHost, callback);

    public static void HookDpiChanged(this Image image, RoutedEventHandler callback) => HookDpiChanged<Image>(image, callback);

    public static void HookDpiChanged(this Window window, RoutedEventHandler callback) => HookDpiChanged<Window>(window, callback);

    private static void HookDpiChanged<T>(T element, RoutedEventHandler callback) where T : FrameworkElement
    {
        Requires.NotNull((object)element, nameof(element));
        Requires.NotNull((object)callback, nameof(callback));
        if (!IsPerMonitorAwarenessEnabled)
            return;
        element.AddHandler(Image.DpiChangedEvent, callback);
    }

    public static IDisposable EnterDpiScope(DpiAwarenessContext awareness)
    {
        return new DpiScope(awareness);
    }

    public static void GetMonitorDpi(this IntPtr hmonitor, out double dpiX, out double dpiY)
    {
        ValidateIntPtr(hmonitor, nameof(hmonitor));
        if (IsPerMonitorAwarenessEnabled)
        {
            var dpi = GetDpiForMonitor(hmonitor);
            dpiX = dpi.X;
            dpiY = dpi.Y;
        }
        else
        {
            dpiX = LazySystemDpiX.Value;
            dpiY = LazySystemDpiY.Value;
        }
        if (!IsValidDpi(dpiX))
            throw new InvalidOperationException("The DPI must be greater than zero.");
        if (!IsValidDpi(dpiY))
            throw new InvalidOperationException("The DPI must be greater than zero.");
    }

    public static double GetWindowDpi(this IntPtr hwnd)
    {
        ValidateIntPtr(hwnd, nameof(hwnd));
        var num = !IsPerMonitorAwarenessEnabled ? LazySystemDpiX.Value : User32.GetDpiForWindow(hwnd);
        return IsValidDpi(num) ? num : throw new InvalidOperationException("The DPI must be greater than zero.");
    }

    public static double GetWindowDpiScale(this IntPtr hwnd) => hwnd.GetWindowDpi() / 96.0;
    
    
    private static Dpi GetDpiForMonitor(IntPtr hmonitor)
    {
        var dpiForMonitor = new Dpi
        {
            HResult = ShCore.GetDpiForMonitor(hmonitor, MonitorDpiType.MdtEffectiveDpi, out var dpiX, out var dpiY)
        };
        if (dpiForMonitor.HResult == 0)
        {
            dpiForMonitor.X = dpiX;
            dpiForMonitor.Y = dpiY;
        }
        return dpiForMonitor;
    }

    private static void ValidateIntPtr(IntPtr ptr, string paramName)
    {
        if (ptr == IntPtr.Zero)
            throw new ArgumentException("Cannot get the DPI of an invalid window.", paramName);
    }

    public static Point PointLogicalPoint(Point point)
    {
        return new Point
        {
            X = point.X * SystemDpiXScale,
            Y = point.Y * SystemDpiYScale
        };
    }

    public static Point DeviceToLogicalPoint(this Visual visual, Point point)
    {
        Requires.NotNull(visual, nameof(visual));
        return new Point
        {
            X = visual.DeviceToLogicalUnitsX(point.X),
            Y = visual.DeviceToLogicalUnitsY(point.Y)
        };
    }


    public static Rect LogicalToDeviceRect(this Window window)
    {
        Requires.NotNull(window, nameof(window));
        return new Rect
        {
            X = window.LogicalToDeviceUnitsX(window.Left),
            Y = window.LogicalToDeviceUnitsY(window.Top),
            Width = window.LogicalToDeviceUnitsX(window.Width),
            Height = window.LogicalToDeviceUnitsY(window.Height)
        };
    }

    public static Rect LogicalToDeviceRect(this IntPtr hwnd, Rect rect)
    {
        ValidateIntPtr(hwnd, nameof(hwnd));
        if (rect == Rect.Empty)
            return rect;
        return new Rect()
        {
            X = hwnd.LogicalToDeviceUnits(rect.X),
            Y = hwnd.LogicalToDeviceUnits(rect.Y),
            Width = hwnd.LogicalToDeviceUnits(rect.Width),
            Height = hwnd.LogicalToDeviceUnits(rect.Height)
        };
    }

    public static Size LogicalToDeviceSize(this Visual visual, Size size)
    {
        Requires.NotNull((object)visual, nameof(visual));
        if (size == Size.Empty)
            return size;
        return new Size
        {
            Width = visual.LogicalToDeviceUnitsX(size.Width),
            Height = visual.LogicalToDeviceUnitsY(size.Height)
        };
    }

    public static int DeviceToLogicalUnitsX(this Visual visual, int value) => visual.DeviceToLogicalUnitsX<int>(value);

    public static int DeviceToLogicalUnitsY(this Visual visual, int value) => visual.DeviceToLogicalUnitsY<int>(value);


    public static double LogicalToDeviceUnits(this IntPtr hwnd, double value) => hwnd.LogicalToDeviceUnits<double>(value);

    public static short LogicalToDeviceUnitsX(this Visual visual, short value) => visual.LogicalToDeviceUnitsX<short>(value);

    public static ushort LogicalToDeviceUnitsX(this Visual visual, ushort value) => visual.LogicalToDeviceUnitsX<ushort>(value);

    public static int LogicalToDeviceUnitsX(this Visual visual, int value) => visual.LogicalToDeviceUnitsX<int>(value);

    public static uint LogicalToDeviceUnitsX(this Visual visual, uint value) => visual.LogicalToDeviceUnitsX<uint>(value);

    public static long LogicalToDeviceUnitsX(this Visual visual, long value) => visual.LogicalToDeviceUnitsX<long>(value);

    public static ulong LogicalToDeviceUnitsX(this Visual visual, ulong value) => visual.LogicalToDeviceUnitsX<ulong>(value);

    public static float LogicalToDeviceUnitsX(this Visual visual, float value) => visual.LogicalToDeviceUnitsX<float>(value);

    public static double LogicalToDeviceUnitsX(this Visual visual, double value) => visual.LogicalToDeviceUnitsX<double>(value);

    public static short LogicalToDeviceUnitsY(this Visual visual, short value) => visual.LogicalToDeviceUnitsY<short>(value);

    public static ushort LogicalToDeviceUnitsY(this Visual visual, ushort value) => visual.LogicalToDeviceUnitsY<ushort>(value);

    public static int LogicalToDeviceUnitsY(this Visual visual, int value) => visual.LogicalToDeviceUnitsY<int>(value);

    public static uint LogicalToDeviceUnitsY(this Visual visual, uint value) => visual.LogicalToDeviceUnitsY<uint>(value);

    public static long LogicalToDeviceUnitsY(this Visual visual, long value) => visual.LogicalToDeviceUnitsY<long>(value);

    public static ulong LogicalToDeviceUnitsY(this Visual visual, ulong value) => visual.LogicalToDeviceUnitsY<ulong>(value);

    public static float LogicalToDeviceUnitsY(this Visual visual, float value) => visual.LogicalToDeviceUnitsY<float>(value);

    public static double LogicalToDeviceUnitsY(this Visual visual, double value) => visual.LogicalToDeviceUnitsY<double>(value);

    private static T LogicalToDeviceUnits<T>(this IntPtr hwnd, T value) where T : IConvertible
    {
        var windowDpiScale = hwnd.GetWindowDpiScale();
        return (T)Convert.ChangeType((value.ToDouble(null) * windowDpiScale), typeof(T));
    }

    private static T DeviceToLogicalUnitsX<T>(this Visual visual, T value) where T : IConvertible => visual.DeviceToLogicalUnits(value, true);

    private static T DeviceToLogicalUnitsY<T>(this Visual visual, T value) where T : IConvertible => visual.DeviceToLogicalUnits(value, false);

    private static T DeviceToLogicalUnits<T>(this Visual visual, T value, bool getDpiScaleX) where T : IConvertible
    {
        var dpiScale = GetDpiScale(visual, getDpiScaleX);
        return (T)Convert.ChangeType(value.ToDouble(null) / dpiScale, typeof(T));
    }

    private static T LogicalToDeviceUnitsX<T>(this Visual visual, T value) where T : IConvertible => visual.LogicalToDeviceUnits<T>(value, true);

    private static T LogicalToDeviceUnitsY<T>(this Visual visual, T value) where T : IConvertible => visual.LogicalToDeviceUnits<T>(value, false);

    private static T LogicalToDeviceUnits<T>(this Visual visual, T value, bool getDpiScaleX) where T : IConvertible
    {
        var dpiScale = GetDpiScale(visual, getDpiScaleX);
        return (T)Convert.ChangeType(value.ToDouble(null) * dpiScale, typeof(T));
    }

    private static double GetDpi(Visual visual, bool getDpiX)
    {
        Requires.NotNull(visual, nameof(visual));
        double num;
        if (IsPerMonitorAwarenessEnabled)
        {
            var dpiScaleObject = VisualTreeHelper.GetDpi(visual);
            num = getDpiX ? dpiScaleObject.PixelsPerInchX : dpiScaleObject.PixelsPerInchY;
        }
        else
            num = getDpiX ? LazySystemDpiX.Value : LazySystemDpiY.Value;
        return IsValidDpi(num) ? num : throw new InvalidOperationException("The DPI must be greater than zero.");
    }

    private static double GetDpiScale(Visual visual, bool getDpiScaleX)
    {
        Requires.NotNull(visual, nameof(visual));
        double num;
        if (IsPerMonitorAwarenessEnabled)
        {
            var dpiScaleObject = VisualTreeHelper.GetDpi(visual);
            num = getDpiScaleX ? dpiScaleObject.DpiScaleX : dpiScaleObject.DpiScaleY;
        }
        else
            num = getDpiScaleX ? LazySystemDpiScaleX.Value : LazySystemDpiScaleY.Value;
        return IsValidDpi(num) ? num : throw new InvalidOperationException("The DPI scale must be greater than zero.");
    }

    private enum DeviceCaps
    {
        LogPixelsX = 88,
        LogPixelsY = 90
    }

    internal enum MonitorDpiType
    {
        MdtEffectiveDpi,
        MdtAngularDpi,
        MdtRawDpi,
        MdtDefault,
    }

    internal enum ProcessDpiAwareness
    {
        ProcessDpiUnaware,
        ProcessSystemDpiAware,
        ProcessPerMonitorDpiAware,
    }

    private class DpiScope : IDisposable
    {
        private Thread _originalThread;
        private IntPtr _originalAwareness = IntPtr.Zero;

        public DpiScope(DpiAwarenessContext awareness)
          : this(new IntPtr((int)awareness))
        {
        }

        private DpiScope(IntPtr awareness)
        {
            _originalThread = Thread.CurrentThread;
            if (!IsPerMonitorAwarenessEnabled)
                return;
            _originalAwareness = User32.SetThreadDpiAwarenessContext(awareness);
        }

        public void Dispose()
        {
            if (_originalThread != Thread.CurrentThread)
                throw new InvalidOperationException("The DPI awareness context must be restored on the same thread on which it was set.");
            if (!IsPerMonitorAwarenessEnabled || !(_originalAwareness != IntPtr.Zero))
                return;
            User32.SetThreadDpiAwarenessContext(_originalAwareness);
            _originalThread = null;
            _originalAwareness = IntPtr.Zero;
        }
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