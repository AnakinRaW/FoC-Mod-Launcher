using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using Sklavenwalker.CommonUtilities.Wpf.NativeMethods;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.DPI;

public static class Screen
{
    private static readonly List<DisplayInfo> Displays = new();

    public static int DisplayCount => Displays.Count;

    static Screen()
    {
        SystemEvents.DisplaySettingsChanged += OnDisplayChanged;
        UpdateDisplays();
    }

    public static void SetInitialWindowRect(IntPtr hwnd, Window window, Int32Rect windowBounds)
    {
        if (hwnd == IntPtr.Zero)
            throw new ArgumentException(nameof(hwnd));
        Requires.NotNull((object)window, nameof(window));
        SetWindowDpi(hwnd, window, windowBounds);
        window.Top = window.DeviceToLogicalUnitsY(windowBounds.Y);
        window.Left = window.DeviceToLogicalUnitsX(windowBounds.X);
        window.Width = window.DeviceToLogicalUnitsX(windowBounds.Width);
        window.Height = window.DeviceToLogicalUnitsY(windowBounds.Height);
    }

    public static int FindDisplayForWindowRect(Rect windowRect)
    {
        var displayForWindowRect = -1;
        var rect = new RectStruct(windowRect);
        long maxIntersectionArea = 0;
        
        for (var index = 0; index < Displays.Count; ++index)
        {
            var rcWork = Displays[index].MonitorInfo.rcWork;
            User32.IntersectRect(out var lprcDst, ref rcWork, ref rect);
            var localIntersectionArea = (long)(lprcDst.Width * lprcDst.Height);
            if (localIntersectionArea > maxIntersectionArea)
            {
                displayForWindowRect = index;
                maxIntersectionArea = localIntersectionArea;
            }
        }

        if (displayForWindowRect == -1)
            displayForWindowRect = FindDisplayForHmonitor(User32.MonitorFromRect(ref rect, 2));
        return displayForWindowRect;
    }

    public static int FindDisplayForAbsolutePosition(Point absolutePosition)
    {
        for (var index = 0; index < Displays.Count; ++index)
        {
            var rect = Displays[index].Rect;
            if (rect.Left <= absolutePosition.X && rect.Right > absolutePosition.X && rect.Top <= absolutePosition.Y &&
                rect.Bottom > absolutePosition.Y)
                return index;
        }

        return FindDisplayForHmonitor(User32.MonitorFromPoint(PointStruct.FromPoint(absolutePosition), 2));
    }

    public static void AbsolutePositionToRelativePosition(double left, double top, out int display, out Point relativePosition)
    {
        display = FindDisplayForAbsolutePosition(new Point(left, top));
        relativePosition = new Point(left, top);
        if (-1 == display)
            return;
        relativePosition = (Point)(relativePosition - Displays[display].Position);
    }


    internal static int FindDisplayForHmonitor(IntPtr hmonitor)
    {
        var index = Displays.FindIndex(HasSameMonitorHandle);
        if (index < 0)
        {
            UpdateDisplays();
            index = Displays.FindIndex(HasSameMonitorHandle);
            if (index < 0)
                throw new ArgumentException(nameof(hmonitor));
        }
        return index;

        bool HasSameMonitorHandle(DisplayInfo displayInfo) => displayInfo != null && displayInfo.MonitorHandle == hmonitor;
    }

    internal static Rect GetOnScreenPosition(Rect floatRect, int fallbackDisplay = -1, bool forceOnScreen = false, bool topOnly = false)
    {
        var windowRect = floatRect;
        FindMaximumSingleMonitorRectangle(windowRect, out var screenSubRect, out var monitorRect);

        if (screenSubRect.Width == 0.0 || screenSubRect.Height == 0.0 || forceOnScreen)
        {
            Rect workAreaRect;
            if (fallbackDisplay == -1)
                FindMonitorRectsFromPoint(User32.GetCursorPos(), out monitorRect, out workAreaRect);
            else
                FindMonitorRectsFromPoint(RelativePositionToAbsolutePosition(fallbackDisplay, 0.0, 0.0), out monitorRect, out workAreaRect);
            if (windowRect.Width > workAreaRect.Width)
                windowRect.Width = workAreaRect.Width;
            if (windowRect.Height > workAreaRect.Height)
                windowRect.Height = workAreaRect.Height;
            if (!topOnly)
            {
                if (windowRect.Right > workAreaRect.Right)
                    windowRect.X = workAreaRect.Right - windowRect.Width;
                if (windowRect.Left < workAreaRect.Left)
                    windowRect.X = workAreaRect.Left;
                if (windowRect.Bottom > workAreaRect.Bottom)
                    windowRect.Y = workAreaRect.Bottom - windowRect.Height;
            }
            if (windowRect.Top < workAreaRect.Top)
                windowRect.Y = workAreaRect.Top;
        }
        return windowRect;
    }

    internal static Point RelativePositionToAbsolutePosition(int display, double left, double top)
    {
        if (display < 0)
            throw new ArgumentOutOfRangeException(nameof(display));
        RectStruct rect1;
        if (display >= Displays.Count)
        {
           var rect2 = Displays[Displays.Count - 1].Rect;
            rect1 = new RectStruct(rect2.Left + rect2.Width, rect2.Top, rect2.Right + rect2.Width, rect2.Bottom);
        }
        else
            rect1 = Displays[display].Rect;
        return new Point(rect1.Left + left, rect1.Top + top);
    }

    internal static void FindMonitorRectsFromRect(Rect rect, out Rect monitorRect, out Rect workAreaRect)
    {
        var displayForWindowRect = FindDisplayForWindowRect(rect);
        var monitorInfo = new MonitorInfoStruct();
        if (displayForWindowRect == -1)
        {
            var rect1 = new RectStruct(rect);
            GetMonitorInfo(User32.MonitorFromRect(ref rect1, 2), ref monitorInfo);
        }
        else
            monitorInfo = Displays[displayForWindowRect].MonitorInfo;
        GetMonitorRects(monitorInfo, out monitorRect, out workAreaRect);
    }

    internal static void FindMonitorRectsFromPoint(Point point, out Rect monitorRect, out Rect workAreaRect)
    {
        var absolutePosition = FindDisplayForAbsolutePosition(point);
        var monitorInfo = new MonitorInfoStruct();
        if (absolutePosition == -1)
            GetMonitorInfo(User32.MonitorFromPoint(PointStruct.FromPoint(point), 2), ref monitorInfo);
        else
            monitorInfo = Displays[absolutePosition].MonitorInfo;
        GetMonitorRects(monitorInfo, out monitorRect, out workAreaRect);
    }

    private static void FindMaximumSingleMonitorRectangle(Rect windowRect, out Rect screenSubRect, out Rect monitorRect)
    {
        FindMaximumSingleMonitorRectangle(new RectStruct(windowRect), out var screenSubRect1, out var monitorRect1);
        screenSubRect = new Rect(screenSubRect1.Position, screenSubRect1.Size);
        monitorRect = new Rect(monitorRect1.Position, monitorRect1.Size);
    }

    private static void FindMaximumSingleMonitorRectangle(RectStruct windowRect, out RectStruct screenSubRect, out RectStruct monitorRect)
    {
        var displayForWindowRect = FindDisplayForWindowRect(windowRect.ToRect());
        screenSubRect = new RectStruct
        {
            Left = 0,
            Right = 0,
            Top = 0,
            Bottom = 0
        };
        monitorRect = new RectStruct
        {
            Left = 0,
            Right = 0,
            Top = 0,
            Bottom = 0
        };
        if (displayForWindowRect == -1)
            return;
        var monitorInfo = Displays[displayForWindowRect].MonitorInfo;
        var rcWork = monitorInfo.rcWork;
        User32.IntersectRect(out var lprcDst, ref rcWork, ref windowRect);
        screenSubRect = lprcDst;
        monitorRect = monitorInfo.rcWork;
    }

    private static void GetMonitorRects(MonitorInfoStruct monitorInfo, out Rect monitorRect, out Rect workAreaRect)
    {
        if (monitorInfo.cbSize != 0U)
        {
            monitorRect = new Rect(monitorInfo.rcMonitor.Position, monitorInfo.rcMonitor.Size);
            workAreaRect = new Rect(monitorInfo.rcWork.Position, monitorInfo.rcWork.Size);
        }
        else
        {
            monitorRect = new Rect(0.0, 0.0, 0.0, 0.0);
            workAreaRect = new Rect(0.0, 0.0, 0.0, 0.0);
        }
    }

    private static void OnDisplayChanged(object sender, EventArgs e)
    {
        UpdateDisplays();
    }

    private static void UpdateDisplays()
    {
        Displays.Clear();
        using (DpiAwareness.EnterDpiScope(DpiAwareness.ProcessDpiAwarenessContext))
        {
            var dpiHwnd = User32.CreateWindowEx(0, "static", "DpiTrackingWindow", 0, 0, 0, 0, 0, IntPtr.Zero,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            User32.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                (IntPtr hMonitor, IntPtr hdcMonitor, ref RectStruct rect, IntPtr lpData) =>
                {
                    var monitorInfo = new MonitorInfoStruct();
                    GetMonitorInfo(hMonitor, ref monitorInfo);
                    var displayInfo = new DisplayInfo(hMonitor, monitorInfo);
                    Displays.Add(displayInfo);
                    return true;
                }, IntPtr.Zero);
            User32.DestroyWindow(dpiHwnd);
        }

        Displays.Sort();
    }

    private static void GetMonitorInfo(IntPtr monitor, ref MonitorInfoStruct monitorInfo)
    {
        if (!(monitor != IntPtr.Zero))
            return;
        monitorInfo.cbSize = (uint)Marshal.SizeOf(typeof(MonitorInfoStruct));
        User32.GetMonitorInfo(monitor, ref monitorInfo);
    }

    private static void SetWindowDpi(IntPtr hwnd, Window window, Int32Rect windowBounds)
    { 
        var rect = RectStruct.FromInt32Rect(windowBounds);
        var monitorDpiScale = GetMonitorDpiScale(rect);
        var wParam = new IntPtr((int)monitorDpiScale.PixelsPerInchX | (int)monitorDpiScale.PixelsPerInchY << 16);
        var num = Marshal.AllocCoTaskMem(Marshal.SizeOf(rect));
        try
        {
            Marshal.StructureToPtr(rect, num, false);
            User32.SendMessage(hwnd, 736, wParam, num);
            VisualTreeHelper.SetRootDpi(window, monitorDpiScale);
        }
        finally
        {
            Marshal.FreeCoTaskMem(num);
        }
    }

    private static DpiScale GetMonitorDpiScale(RectStruct nativeRect)
    {
        var displayForWindowRect = FindDisplayForWindowRect(nativeRect.ToRect());
        return new DpiScale(LogicalToDeviceUnitsX(displayForWindowRect, 1.0), LogicalToDeviceUnitsY(displayForWindowRect, 1.0));
    }

    public static double LogicalToDeviceUnitsX(int display, double value)
    {
        GetMonitorDpi(display, out _, out var dpiY);
        return ScaleLogicalToDevice(dpiY, value);
    }

    public static double LogicalToDeviceUnitsY(int display, double value)
    {
        GetMonitorDpi(display, out _, out var dpiY);
        return ScaleLogicalToDevice(dpiY, value);
    }

    private static double ScaleLogicalToDevice(double dpi, double value) => value * dpi / 96.0;

    private static void GetMonitorDpi(int display, out double dpiX, out double dpiY)
    {
        try
        {
            Displays[display].MonitorHandle.GetMonitorDpi(out dpiX, out dpiY);
        }
        catch (InvalidOperationException)
        {
            dpiX = 96.0;
            dpiY = 96.0;
        }
    }

    private class DisplayInfo : IComparable<DisplayInfo>
    {
        private double DpiX { get; }

        private double DpiY { get; }

        private bool IsDpiFaulted { get; }

        public bool IsPrimary => MonitorInfo.IsPrimary;

        public IntPtr MonitorHandle { get; }

        public MonitorInfoStruct MonitorInfo { get; }

        public Point Position => MonitorInfo.rcMonitor.Position;

        public RectStruct Rect => MonitorInfo.rcMonitor;

        public Size Size => MonitorInfo.rcMonitor.Size;

        private PolarVector Vector { get; }

        public DisplayInfo(IntPtr hMonitor, MonitorInfoStruct monitorInfo)
        {
            MonitorHandle = hMonitor;
            MonitorInfo = monitorInfo;
            try
            {
                hMonitor.GetMonitorDpi(out var dpiX, out var dpiY);
                DpiX = dpiX;
                DpiY = dpiY;
            }
            catch
            {
                DpiX = 96.0;
                DpiY = 96.0;
                IsDpiFaulted = true;
            }
            Vector = new PolarVector(Position);
        }
        public int CompareTo(DisplayInfo other)
        {
            if (Vector.IsOrigin && !other.Vector.IsOrigin)
                return -1;
            if (!Vector.IsOrigin && other.Vector.IsOrigin)
                return 1;
            if (Vector.Angle < other.Vector.Angle)
                return -1;
            if (Vector.Angle > other.Vector.Angle)
                return 1;
            if (Vector.Length < other.Vector.Length)
                return -1;
            return Vector.Length > other.Vector.Length ? 1 : 0;
        }
    }

    private class PolarVector
    {
        public PolarVector(Point topLeft)
        {
            Angle = Math.Atan2(topLeft.Y, topLeft.X) * (180.0 / Math.PI);
            Length = new Vector(topLeft.X, topLeft.Y).Length;
        }

        public bool IsOrigin => Length == 0.0 && Angle == 0.0;

        public double Angle { get; }

        public double Length { get; }

        public override string ToString() => $"{Length}, {Angle}";
    }
}