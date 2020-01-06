using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using FocLauncherApp.NativeMethods;

namespace FocLauncherApp.ScreenUtilities
{
    internal static class Screen
    {
        public static event EventHandler DisplayConfigChanged;

        private static readonly List<DisplayInfo> Displays = new List<DisplayInfo>();

        public static int DisplayCount => Displays.Count;

        static Screen()
        {
            BroadcastMessageMonitor.Instance.DisplayChange += OnDisplayChange;
            UpdateDisplays();
        }

        public static double LogicalToDeviceUnitsX(int display, double value)
        {
            GetMonitorDpi(display, out var dpiX, out var dpiY);
            return ScaleLogicalToDevice(dpiY, value);
        }

        public static double LogicalToDeviceUnitsY(int display, double value)
        {
            GetMonitorDpi(display, out var dpiX, out var dpiY);
            return ScaleLogicalToDevice(dpiY, value);
        }

        public static void SetInitialWindowRect(IntPtr hwnd, Window window, Int32Rect windowBounds)
        {
            if (hwnd == IntPtr.Zero)
                throw new ArgumentException(nameof(hwnd));
            if (window == null)
                throw new ArgumentNullException(nameof(window));
            SetWindowDpi(hwnd, window, windowBounds);
            window.Top = window.DeviceToLogicalUnitsY(windowBounds.Y);
            window.Left = window.DeviceToLogicalUnitsX(windowBounds.X);
            window.Width = window.DeviceToLogicalUnitsX(windowBounds.Width);
            window.Height = window.DeviceToLogicalUnitsY(windowBounds.Height);
        }

        public static int FindDisplayForWindowRect(Rect windowRect)
        {
            var num1 = -1;
            var lprcSrc2 = new RECT(windowRect);
            long num2 = 0;
            for (var index = 0; index < Displays.Count; ++index)
            {
                var rcWork = Displays[index].MonitorInfo.RcWork;
                NativeMethods.NativeMethods.IntersectRect(out var lprcDst, ref rcWork, ref lprcSrc2);
                long num3 = lprcDst.Width * lprcDst.Height;
                if (num3 > num2)
                {
                    num1 = index;
                    num2 = num3;
                }
            }
            if (-1 == num1)
            {
                var num3 = double.MaxValue;
                for (var index = 0; index < Displays.Count; ++index)
                {
                    var num4 = Distance(Displays[index].MonitorInfo.RcMonitor, lprcSrc2);
                    if (num4 < num3)
                    {
                        num1 = index;
                        num3 = num4;
                    }
                }
            }
            return num1;
        }

        public static int FindDisplayForAbsolutePosition(Point absolutePosition)
        {
            for (var index = 0; index < Displays.Count; ++index)
            {
                var rcMonitor = Displays[index].MonitorInfo.RcMonitor;
                if (rcMonitor.Left <= absolutePosition.X && rcMonitor.Right >= absolutePosition.X && (rcMonitor.Top <= absolutePosition.Y && rcMonitor.Bottom >= absolutePosition.Y))
                    return index;
            }
            var num1 = -1;
            var num2 = double.MaxValue;
            for (var index = 0; index < Displays.Count; ++index)
            {
                var num3 = Distance(absolutePosition, Displays[index].MonitorInfo.RcMonitor);
                if (num3 < num2)
                {
                    num1 = index;
                    num2 = num3;
                }
            }
            return num1;
        }

        private static void OnDisplayChange(object sender, EventArgs e)
        {
            UpdateDisplays();
        }

        private static double ScaleLogicalToDevice(double dpi, double value)
        {
            return value * dpi / 96.0;
        }

        private static double Distance(Point point, RECT rect)
        {
            return Distance(point, GetRectCenter(rect));
        }

        private static double Distance(RECT rect1, RECT rect2)
        {
            return Distance(GetRectCenter(rect1), GetRectCenter(rect2));
        }

        private static double Distance(Point point1, Point point2)
        {
            return Math.Sqrt(Math.Pow(point1.X - point2.X, 2.0) + Math.Pow(point1.Y - point2.Y, 2.0));
        }

        private static Point GetRectCenter(RECT rect)
        {
            return new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
        }

        private static void UpdateDisplays()
        {
            Displays.Clear();
            NativeMethods.NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT rect, IntPtr lpData) =>
            {
                var monitorInfo = new MonitorInfo { CbSize = (uint)Marshal.SizeOf(typeof(MonitorInfo)) };
                if (NativeMethods.NativeMethods.GetMonitorInfo(hMonitor, ref monitorInfo))
                    Displays.Add(new DisplayInfo(hMonitor, monitorInfo));
                return true;
            }, IntPtr.Zero);
            DisplayConfigChanged?.Invoke(null, EventArgs.Empty);
        }

        private static DpiScale GetMonitorDpiScale(double x, double y)
        {
            var absolutePosition = FindDisplayForAbsolutePosition(new Point(x, y));
            return new DpiScale(LogicalToDeviceUnitsX(absolutePosition, 1.0), LogicalToDeviceUnitsY(absolutePosition, 1.0));
        }

        private static void SetWindowDpi(IntPtr hwnd, Window window, Int32Rect windowBounds)
        {
            var structure = RECT.FromInt32Rect(windowBounds);
            var monitorDpiScale = GetMonitorDpiScale(windowBounds.X, windowBounds.Y);
            var wParam = new IntPtr((int)monitorDpiScale.PixelsPerInchX | (int)monitorDpiScale.PixelsPerInchY << 16);
            var num = Marshal.AllocCoTaskMem(Marshal.SizeOf(structure));
            try
            {
                Marshal.StructureToPtr(structure, num, false);
                NativeMethods.NativeMethods.SendMessage(hwnd, 736, wParam, num);
                VisualTreeHelper.SetRootDpi(window, monitorDpiScale);
            }
            finally
            {
                Marshal.FreeCoTaskMem(num);
            }
        }

        private static void GetMonitorDpi(int display, out double dpiX, out double dpiY)
        {
            try
            {
                Displays[display].MonitorHandle.GetMonitorDpi(out dpiX, out dpiY);
            }
            catch (MonitorDpiAwarenessException)
            {
                dpiX = 96.0;
                dpiY = 96.0;
            }
        }

        private class DisplayInfo
        {
            public IntPtr MonitorHandle { get; }

            public MonitorInfo MonitorInfo { get; }

            public DisplayInfo(IntPtr hMonitor, MonitorInfo monitorInfo)
            {
                MonitorHandle = hMonitor;
                MonitorInfo = monitorInfo;
            }
        }
    }
}