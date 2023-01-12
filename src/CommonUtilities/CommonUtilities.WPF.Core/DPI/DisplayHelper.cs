using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using AnakinRaW.CommonUtilities.Wpf.Utilities;
using Microsoft.Win32;
using Validation;
using Vanara.PInvoke;

namespace AnakinRaW.CommonUtilities.Wpf.DPI;

public static class DisplayHelper
{
    private static readonly List<DisplayInfo> DisplaysInternal = new();

    public static IReadOnlyList<DisplayInfo> Displays => DisplaysInternal.ToList();
    
    static DisplayHelper()
    {
        SystemEvents.DisplaySettingsChanged += OnDisplayChanged!;
        UpdateDisplays();
    }

    public static Point DeviceToLogicalPoint(DisplayInfo display, Point point)
    {
        GetMonitorDpi(display, out var dpiX, out var dpiY);
        return new Point
        {
            X = ScaleDeviceToLogical(dpiX, point.X),
            Y = ScaleDeviceToLogical(dpiY, point.Y)
        };
    }

    public static Rect DeviceToLogicalRect(DisplayInfo display, Rect rect)
    {
        if (rect == Rect.Empty)
            return rect;
        GetMonitorDpi(display, out var dpiX, out var dpiY);
        return new Rect
        {
            X = ScaleDeviceToLogical(dpiX, rect.X),
            Y = ScaleDeviceToLogical(dpiY, rect.Y),
            Width = ScaleDeviceToLogical(dpiX, rect.Width),
            Height = ScaleDeviceToLogical(dpiY, rect.Height)
        };
    }

    public static Size DeviceToLogicalSize(DisplayInfo display, Size size)
    {
        if (size == Size.Empty)
            return size;
        GetMonitorDpi(display, out var dpiX, out var dpiY);
        return new Size
        {
            Width = ScaleDeviceToLogical(dpiX, size.Width),
            Height = ScaleDeviceToLogical(dpiY, size.Height)
        };
    }

    public static double DeviceToLogicalUnitsX(DisplayInfo display, double value)
    {
        GetMonitorDpi(display, out var dpiX, out var _);
        return ScaleDeviceToLogical(dpiX, value);
    }

    public static double DeviceToLogicalUnitsY(DisplayInfo display, double value)
    {
        GetMonitorDpi(display, out var _, out var dpiY);
        return ScaleDeviceToLogical(dpiY, value);
    }

    public static Point LogicalToDevicePoint(DisplayInfo display, Point point)
    {
        GetMonitorDpi(display, out var dpiX, out var dpiY);
        return new Point
        {
            X = ScaleLogicalToDevice(dpiX, point.X),
            Y = ScaleLogicalToDevice(dpiY, point.Y)
        };
    }

    public static Rect LogicalToDeviceRect(DisplayInfo display, Rect rect)
    {
        if (rect == Rect.Empty)
            return rect;
        GetMonitorDpi(display, out var dpiX, out var dpiY);
        return new Rect
        {
            X = ScaleLogicalToDevice(dpiX, rect.X),
            Y = ScaleLogicalToDevice(dpiY, rect.Y),
            Width = ScaleLogicalToDevice(dpiX, rect.Width),
            Height = ScaleLogicalToDevice(dpiY, rect.Height)
        };
    }

    public static Size LogicalToDeviceSize(DisplayInfo display, Size size)
    {
        if (size == Size.Empty)
            return size;
        GetMonitorDpi(display, out var dpiX, out var dpiY);
        return new Size
        {
            Width = ScaleLogicalToDevice(dpiX, size.Width),
            Height = ScaleLogicalToDevice(dpiY, size.Height)
        };
    }

    public static double LogicalToDeviceUnitsX(DisplayInfo display, double value)
    {
        GetMonitorDpi(display, out double _, out var dpiY);
        return ScaleLogicalToDevice(dpiY, value);
    }

    public static double LogicalToDeviceUnitsY(DisplayInfo display, double value)
    {
        GetMonitorDpi(display, out double _, out var dpiY);
        return ScaleLogicalToDevice(dpiY, value);
    }

    public static void SetWindowRect(Window window, Int32Rect windowBounds)
    {
        var hwnd = new WindowInteropHelper(window).EnsureHandle();
        if (hwnd == IntPtr.Zero)
            throw new ArgumentException(nameof(hwnd));
        Requires.NotNull((object)window, nameof(window));
        SetWindowDpi(hwnd, window, windowBounds);
        window.Top = window.DeviceToLogicalUnitsY(windowBounds.Y);
        window.Left = window.DeviceToLogicalUnitsX(windowBounds.X);
        window.Width = window.DeviceToLogicalUnitsX(windowBounds.Width);
        window.Height = window.DeviceToLogicalUnitsY(windowBounds.Height);
    }

    public static DisplayInfo FindDisplayForWindowRect(Rect windowRect)
    {
        DisplayInfo? displayForWindowRect = null;
        var rect = windowRect.ToRECT();
        long maxIntersectionArea = 0;
        
        foreach (var displayInfo in DisplaysInternal)
        {
            var rcWork = displayInfo.MonitorInfo.rcWork;
            User32.IntersectRect(out var lprcDst, in rcWork, in rect);
            var localIntersectionArea = (long)(lprcDst.Width * lprcDst.Height);
            if (localIntersectionArea > maxIntersectionArea)
            {
                displayForWindowRect = displayInfo;
                maxIntersectionArea = localIntersectionArea;
            }
        }
        
        return displayForWindowRect ?? FindDisplayForHmonitor(User32.MonitorFromRect(in rect, User32.MonitorFlags.MONITOR_DEFAULTTONEAREST).DangerousGetHandle());
    }

    public static DisplayInfo? FindDisplayForAbsolutePosition(Point absolutePosition)
    {
        foreach (var display in DisplaysInternal)
        {
            var rect = display.Rect;
            if (rect.Left <= absolutePosition.X && rect.Right > absolutePosition.X && rect.Top <= absolutePosition.Y &&
                rect.Bottom > absolutePosition.Y)
                return display;
        }
        return FindDisplayForHmonitor(User32.MonitorFromPoint(absolutePosition.ToPOINT(), User32.MonitorFlags.MONITOR_DEFAULTTONEAREST));
    }

    public static void AbsolutePositionToRelativePosition(double left, double top, out DisplayInfo? display, out Point relativePosition)
    {
        display = FindDisplayForAbsolutePosition(new Point(left, top));
        relativePosition = new Point(left, top);
        if (display is null)
            return;
        relativePosition = (Point)(relativePosition - display.Position);
    }

    private static DisplayInfo FindDisplayForHmonitor(HMONITOR hmonitor)
    {
        var display = DisplaysInternal.Find(HasSameMonitorHandle);
        if (display is null)
        {
            UpdateDisplays();
            display = DisplaysInternal.Find(HasSameMonitorHandle);
            if (display is null)
                throw new ArgumentException(nameof(hmonitor));
        }
        return display;

        bool HasSameMonitorHandle(DisplayInfo? displayInfo)
        {
            return displayInfo != null && displayInfo.MonitorHandle == hmonitor.DangerousGetHandle();
        }
    }
    
    internal static Rect GetOnScreenPosition(Rect windowRect, 
        DisplayInfo? fallbackDisplay = null, bool forceOnScreen = false, bool topOnly = false)
    {
        FindMaximumSingleMonitorRectangle(windowRect, out var screenSubRect, out _);

        if (screenSubRect.Width == 0.0 || screenSubRect.Height == 0.0 || forceOnScreen)
        {
            Rect workAreaRect;
            if (fallbackDisplay is null)
                FindMonitorRectsFromPoint(GetCursorPos(), out _, out workAreaRect);
            else
                FindMonitorRectsFromPoint(RelativePositionToAbsolutePosition(fallbackDisplay, 0.0, 0.0), out _, out workAreaRect);
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

    internal static Point RelativePositionToAbsolutePosition(DisplayInfo display, double left, double top)
    {
        if (display is null) throw new ArgumentNullException(nameof(display));
        RECT rect1;
        if (DisplaysInternal.Contains(display))
        {
            var rect2 = DisplaysInternal.Last().Rect;
            rect1 = new RECT(rect2.Left + rect2.Width, rect2.Top, rect2.Right + rect2.Width, rect2.Bottom);
        }
        else
            rect1 = display.Rect;
        return new Point(rect1.Left + left, rect1.Top + top);
    }

    internal static void FindMonitorRectsFromPoint(Point point, out Rect monitorRect, out Rect workAreaRect)
    {
        var display = FindDisplayForAbsolutePosition(point);
        var monitorInfo = new User32.MONITORINFO();
        if (display is null)
            GetMonitorInfo(User32.MonitorFromPoint(point.ToPOINT(), User32.MonitorFlags.MONITOR_DEFAULTTONEAREST), ref monitorInfo);
        else
            monitorInfo = display.MonitorInfo;
        GetMonitorRects(monitorInfo, out monitorRect, out workAreaRect);
    }

    private static void FindMaximumSingleMonitorRectangle(Rect windowRect, out Rect screenSubRect, out Rect monitorRect)
    {
        FindMaximumSingleMonitorRectangle(windowRect.ToRECT(), out var screenSubRect1, out var monitorRect1);
        screenSubRect = new Rect(screenSubRect1.GetPosition(), screenSubRect1.ToSize());
        monitorRect = new Rect(monitorRect1.GetPosition(), monitorRect1.ToSize());
    }

    private static void FindMaximumSingleMonitorRectangle(RECT windowRect, out RECT screenSubRect, out RECT monitorRect)
    {
        var display = FindDisplayForWindowRect(windowRect.ToWpfRect());
        screenSubRect = new RECT();
        monitorRect = new RECT();
        var monitorInfo = display.MonitorInfo;
        var rcWork = monitorInfo.rcWork;
        User32.IntersectRect(out var lprcDst, in rcWork, in windowRect);
        screenSubRect = lprcDst;
        monitorRect = monitorInfo.rcWork;
    }

    private static void GetMonitorRects(User32.MONITORINFO monitorInfo, out Rect monitorRect, out Rect workAreaRect)
    {
        if (monitorInfo.cbSize != 0)
        {
            monitorRect = new Rect(monitorInfo.rcMonitor.GetPosition(), monitorInfo.rcMonitor.ToSize());
            workAreaRect = new Rect(monitorInfo.rcWork.GetPosition(), monitorInfo.rcWork.ToSize());
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
        DisplaysInternal.Clear();
        using (DpiHelper.EnterDpiScope(DpiHelper.ProcessDpiAwarenessContext))
        {
            var dpiHwnd = User32.CreateWindowEx(0, "static", "DpiTrackingWindow");
            User32.EnumDisplayMonitors(IntPtr.Zero, null, 
                (hMonitor, _, _, _) =>
                {
                    var monitorInfo = new User32.MONITORINFO();
                    GetMonitorInfo(hMonitor, ref monitorInfo);
                    var displayInfo = new DisplayInfo(hMonitor, monitorInfo);
                    DisplaysInternal.Add(displayInfo);
                    return true;
                }, IntPtr.Zero);
            User32.DestroyWindow(dpiHwnd);
        }

        DisplaysInternal.Sort();
    }

    private static void GetMonitorInfo(HMONITOR monitor, ref User32.MONITORINFO monitorInfo)
    {
        if (!(monitor != IntPtr.Zero))
            return;
        monitorInfo.cbSize = (uint)Marshal.SizeOf(typeof(User32.MONITORINFO));
        User32.GetMonitorInfo(monitor, ref monitorInfo);
    }

    private static void SetWindowDpi(IntPtr hwnd, Window window, Int32Rect windowBounds)
    {
        var rect = windowBounds.AsRECT();
        var monitorDpiScale = GetMonitorDpiScale(rect);
        var wParam = new IntPtr((int)monitorDpiScale.PixelsPerInchX | (int)monitorDpiScale.PixelsPerInchY << 16);
        var num = Marshal.AllocCoTaskMem(Marshal.SizeOf(rect));
        try
        {
            Marshal.StructureToPtr(rect, num, false);
            User32.SendMessage(hwnd, User32.WindowMessage.WM_DPICHANGED);
            VisualTreeHelper.SetRootDpi(window, monitorDpiScale);
        }
        finally
        {
            Marshal.FreeCoTaskMem(num);
        }
    }

    private static DpiScale GetMonitorDpiScale(RECT nativeRect)
    {
        var displayForWindowRect = FindDisplayForWindowRect(nativeRect.ToWpfRect());
        return new DpiScale(LogicalToDeviceUnitsX(displayForWindowRect, 1.0), LogicalToDeviceUnitsY(displayForWindowRect, 1.0));
    }

    private static double ScaleDeviceToLogical(double dpi, double value)
    {
        return value * 96.0 / dpi;
    }

    private static double ScaleLogicalToDevice(double dpi, double value)
    {
        return value * dpi / 96.0;
    }

    private static void GetMonitorDpi(DisplayInfo display, out double dpiX, out double dpiY)
    {
        try
        {
            display.MonitorHandle.GetMonitorDpi(out dpiX, out dpiY);
        }
        catch (InvalidOperationException)
        {
            dpiX = 96.0;
            dpiY = 96.0;
        }
    }
    
    private static Point GetCursorPos()
    {
        var cursorPos = new Point();
        if (User32.GetCursorPos(out var point))
        {
            cursorPos.X = point.X;
            cursorPos.Y = point.Y;
        }
        return cursorPos;
    }
}