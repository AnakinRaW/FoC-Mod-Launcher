using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Sklavenwalker.CommonUtilities.Wpf.DPI;
using Sklavenwalker.CommonUtilities.Wpf.NativeMethods;
using Sklavenwalker.CommonUtilities.Wpf.Utilities;
using Sklavenwalker.CommonUtilities.Wpf.Utils;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public class ShadowChromeWindow : Window
{
    public static readonly Size LogicalResizeBorder = new(6.0, 6.0);
    private IntPtr _ownerForActivate;
    private bool _isDragging;

    public static readonly DependencyProperty ActiveGlowColorProperty = DependencyProperty.Register(
        nameof(ActiveGlowColor), typeof(Color), typeof(ShadowChromeWindow),
        new FrameworkPropertyMetadata(Colors.Transparent, OnGlowColorChanged));

    public static readonly DependencyProperty InactiveGlowColorProperty = DependencyProperty.Register(
        nameof(InactiveGlowColor), typeof(Color), typeof(ShadowChromeWindow),
        new FrameworkPropertyMetadata(Colors.Transparent, OnGlowColorChanged));

    private static readonly DependencyPropertyKey DwmOwnsBorderPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(DwmOwnsBorder), typeof(bool), typeof(ShadowChromeWindow),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender, DwmOwnsBorderChanged));

    public static readonly DependencyProperty IsResizableProperty = DependencyProperty.Register(
        nameof(IsResizable), typeof(bool), typeof(ShadowChromeWindow),
        new PropertyMetadata(true, PropertyChangedCallback));

    public static readonly DependencyProperty HasMaximizeButtonProperty = DependencyProperty.Register(
        nameof(HasMaximizeButton), typeof(bool), typeof(ShadowChromeWindow),
        new PropertyMetadata(default(bool), OnWindowStyleChanged));

    public static readonly DependencyProperty HasMinimizeButtonProperty = DependencyProperty.Register(
        nameof(HasMinimizeButton), typeof(bool), typeof(ShadowChromeWindow),
        new PropertyMetadata(default(bool), OnWindowStyleChanged));

    private static void OnWindowStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        var window = (ShadowChromeWindow)obj;
        if (!(PresentationSource.FromVisual(window) is HwndSource hwndSource))
            return;
        window.UpdateWindowStyle(hwndSource.Handle);
    }

    private void UpdateWindowStyle(IntPtr hwnd)
    {
        var windowLong = User32.GetWindowLong(hwnd, GWL.Style);
        var num1 = !HasMaximizeButton ? windowLong & -65537 : windowLong | 65536;
        var num2 = !HasMinimizeButton ? num1 & -131073 : num1 | 131072;
        User32.SetWindowLong(hwnd, -16, num2);
    }

    public bool HasMinimizeButton
    {
        get => (bool)GetValue(HasMinimizeButtonProperty);
        set => SetValue(HasMinimizeButtonProperty, value);
    }

    public bool HasMaximizeButton
    {
        get => (bool)GetValue(HasMaximizeButtonProperty);
        set => SetValue(HasMaximizeButtonProperty, value);
    }


    public static readonly DependencyProperty DwmOwnsBorderProperty =
        DwmOwnsBorderPropertyKey.DependencyProperty;

    private Rect _logicalSizeForRestore = Rect.Empty;
    private bool _useLogicalSizeForRestore;
    private bool _updatingZOrder;

    protected WindowInteropHelper WindowHelper { get; }

    private NonClientButtonManager NCButtonManager { get; }

    private ShadowBorder? LeftBorder { get; set; }

    private ShadowBorder? RightBorder { get; set; }

    private ShadowBorder? BottomBorder { get; set; }

    public bool IsResizable
    {
        get => (bool)GetValue(IsResizableProperty);
        set => SetValue(IsResizableProperty, value);
    }

    public Color ActiveGlowColor
    {
        get => (Color)GetValue(ActiveGlowColorProperty);
        set => SetValue(ActiveGlowColorProperty, value);
    }

    public Color InactiveGlowColor
    {
        get => (Color)GetValue(InactiveGlowColorProperty);
        set => SetValue(InactiveGlowColorProperty, value);
    }

    public bool DwmOwnsBorder
    {
        get => (bool)GetValue(DwmOwnsBorderProperty);
        private set => SetValue(DwmOwnsBorderPropertyKey, value);
    }

    static ShadowChromeWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ShadowChromeWindow),
            new FrameworkPropertyMetadata(typeof(ShadowChromeWindow)));
    }

    public ShadowChromeWindow()
    {
        WindowHelper = new WindowInteropHelper(this);
        NCButtonManager = new NonClientButtonManager(this);
    }

    private static void OnGlowColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        var customChromeWindow = (ShadowChromeWindow)obj;
        var maximized = customChromeWindow.WindowState == WindowState.Maximized;
        customChromeWindow.UpdateGlowBorder(customChromeWindow.IsActive, maximized);
    }

    private static void DwmOwnsBorderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var window = (ShadowChromeWindow)d;
        if ((bool)e.NewValue)
        {
            window.LeftBorder?.Close();
            window.RightBorder?.Close();
            window.BottomBorder?.Close();
            window.LeftBorder = null;
            window.RightBorder = null;
            window.BottomBorder = null;
        }
        else
        {
            window.WindowHelper.EnsureHandle();
            window.LeftBorder = new ShadowBorder(window);
            window.RightBorder = new ShadowBorder(window);
            window.BottomBorder = new ShadowBorder(window);
            window.LeftBorder.Show();
            window.RightBorder.Show();
            window.BottomBorder.Show();
        }
    }

    private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var window = (Window)d;
        if (PresentationSource.FromVisual(window) is not HwndSource hwndSource)
            return;
        UpdateResizable(window, hwndSource.Handle, (bool)e.NewValue);
    }

    private static void UpdateResizable(Window window, IntPtr hwnd, bool resizable)
    {
        var windowLong = User32.GetWindowLong(hwnd, GWL.Style);
        if (resizable)
            windowLong |= 262144;
        else
            windowLong &= ~262144;
        User32.SetWindowLong(hwnd, (short)GWL.Style, windowLong);
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        HwndSource.FromHwnd(WindowHelper.Handle)!.AddHook(HwndSourceHook);
    }

    protected void UpdateGlowBorder(bool activate, bool maximized)
    {
        if (WindowHelper.Handle == IntPtr.Zero)
            return;
        var color = activate ? ActiveGlowColor : InactiveGlowColor;
        var colorRef = new ColorRef(color);
        var attrValue = maximized ? -2 : (int)colorRef.DwColor;
        DwmOwnsBorder = DwmApi.DwmSetWindowAttribute(WindowHelper.Handle,
            DwmWindowAttribute.DwmwaBorderColor, ref attrValue, 4) == 0;
        if (DwmOwnsBorder)
            return;
        var solidColorBrush = new SolidColorBrush(color);
        solidColorBrush.Freeze();
        BorderBrush = solidColorBrush;
    }

    protected void EnsureOnScreen()
    {
        var deviceRect = this.LogicalToDeviceRect();
        var displayForWindowRect = DisplayHelper.FindDisplayForWindowRect(deviceRect);
        var onScreenPosition = DisplayHelper.GetOnScreenPosition(deviceRect, displayForWindowRect, true);
        User32.SetWindowPos(WindowHelper.Handle, IntPtr.Zero, (int)onScreenPosition.Left,
            (int)onScreenPosition.Top, (int)onScreenPosition.Width, (int)onScreenPosition.Height, 20);
    }

    protected virtual IntPtr HwndSourceHook(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case 2:
                var hwndSource = HwndSource.FromHwnd(hWnd);
                hwndSource?.RemoveHook(HwndSourceHook);

                break;
            case 6:
                WmActivate(hWnd, wParam, lParam);
                break;
            case 12:
            case 128: 
                return CallDefWindowProcWithoutRedraw(hWnd, msg, wParam, lParam, ref handled);
            case 70:
                WmWindowPosChanging(hWnd, lParam);
                break;
            case 71:
                WmWindowPosChanged(hWnd, lParam);
                break;
            case 124:
                return WmStyleChanging(wParam, lParam);
            case 131:
                return WmNcCalcSize(hWnd, wParam, lParam, ref handled);
            case 132:
                return WmNcHitTest(hWnd, lParam, ref handled);
            case 160:
                handled = NCButtonManager.HoverTrackedButton(lParam);
                break;
            case 161:
                handled = NCButtonManager.PressTrackedButton(lParam);
                break;
            case 162:
                handled = NCButtonManager.ClickTrackedButton(lParam);
                break;
            case 164:
            case 165:
            case 166:
                RaiseNonClientMouseMessageAsClient(hWnd, msg, wParam, lParam);
                handled = true;
                break;
            case 274:
                WmSysCommand(hWnd, wParam, lParam);
                break;
            case 512:
            case 674:
            case 675:
                NCButtonManager.ClearTrackedButton();
                break;
            case 561:
                _isDragging = true;
                break;
            case 562:
                _isDragging = false;
                break;
        }

        return IntPtr.Zero;
    }

    private static void RaiseNonClientMouseMessageAsClient(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
    {
        var point = new PointStruct
        {
            X = User32.GetXLParam(lParam.ToInt32Unchecked()),
            Y = User32.GetYLParam(lParam.ToInt32Unchecked())
        };
        User32.ScreenToClient(hWnd, ref point);
        User32.SendMessage(hWnd, msg + 513 - 161, new IntPtr(PressedMouseButtons),
            User32.MakeParam(point.X, point.Y));
    }

    private static int PressedMouseButtons
    {
        get
        {
            var pressedMouseButtons = 0;
            if (User32.IsKeyPressed(1))
                pressedMouseButtons |= 1;
            if (User32.IsKeyPressed(2))
                pressedMouseButtons |= 2;
            if (User32.IsKeyPressed(4))
                pressedMouseButtons |= 16;
            if (User32.IsKeyPressed(5))
                pressedMouseButtons |= 32;
            if (User32.IsKeyPressed(6))
                pressedMouseButtons |= 64;
            return pressedMouseButtons;
        }
    }

    private static IntPtr CallDefWindowProcWithoutRedraw(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        using (new SuppressRedrawScope(hWnd))
        {
            handled = true;
            return User32.DefWindowProc(hWnd, msg, wParam, lParam);
        }
    }

    private void WmActivate(IntPtr hWnd, IntPtr wParam, IntPtr lParam)
    {
        if (_ownerForActivate != IntPtr.Zero)
            User32.SendMessage(_ownerForActivate, User32.NotifyOwnerActive, wParam, lParam);
        UpdateGlowBorder(Convert.ToBoolean(wParam.ToInt64()), User32.IsZoomed(hWnd));
    }

    private IntPtr WmNcCalcSize(IntPtr hWnd, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        handled = true;
        var structure = (RectStruct)Marshal.PtrToStructure(lParam, typeof(RectStruct));
        var maximized = User32.IsZoomed(hWnd);
        UpdateGlowBorder(IsActive, maximized);
        if (maximized)
        {
            User32.DefWindowProc(hWnd, 131, wParam, lParam);
            structure = (RectStruct)Marshal.PtrToStructure(lParam, typeof(RectStruct));
            structure.Top -= (int)Math.Ceiling(this.LogicalToDeviceUnitsY(SystemParameters.CaptionHeight) + 1.0);
            var monitorInfo = MonitorInfoFromWindow(hWnd);
            if (monitorInfo.RcMonitor.Height == monitorInfo.RcWork.Height &&
                monitorInfo.RcMonitor.Width == monitorInfo.RcWork.Width)
            {
                var pData = new AppBarData();
                pData.CbSize = Marshal.SizeOf(typeof(AppBarData));
                switch (Convert.ToBoolean(
                            (int)Shell32.SHAppBarMessage(5U, ref pData))
                            ? (int)pData.UEdge
                            : -1)
                {
                    case 0:
                        ++structure.Left;
                        break;
                    case 1:
                        ++structure.Top;
                        break;
                    case 2:
                        --structure.Right;
                        break;
                    case 3:
                        --structure.Bottom;
                        break;
                }
            }
        }
        else
        {
            var deviceSize = this.LogicalToDeviceSize(LogicalResizeBorder);
            structure.Left += (int)deviceSize.Width;
            structure.Right -= (int)deviceSize.Width;
            structure.Bottom -= (int)deviceSize.Height;
        }

        Marshal.StructureToPtr(structure, lParam, true);
        return IntPtr.Zero;
    }

    private IntPtr WmNcHitTest(IntPtr hWnd, IntPtr lParam, ref bool handled)
    {
        if (!this.IsConnectedToPresentationSource())
            return new IntPtr(0);
        var point1 = new Point(User32.GetXLParam(lParam.ToInt32Unchecked()),
            User32.GetYLParam(lParam.ToInt32Unchecked()));
        var point2 = PointFromScreen(point1);
        DependencyObject? visualHit = null;
        UtilityMethods.HitTestVisibleElements(this, target =>
        {
            visualHit = target.VisualHit;
            return HitTestResultBehavior.Stop;
        }, new PointHitTestParameters(point2));
        var num1 = 0;
        for (; visualHit != null; visualHit = visualHit.GetVisualOrLogicalParent())
        {
            if (visualHit is INonClientArea nonClientArea)
            {
                num1 = nonClientArea.HitTest(point1);
                if (num1 != 0)
                    break;
            }
        }

        if (num1 == 0)
        {
            if (point2.Y <= BorderThickness.Top)
            {
                if (WindowState == WindowState.Maximized)
                {
                    num1 = 1;
                }
                else
                {
                    var x1 = point2.X;
                    var logicalResizeBorder = LogicalResizeBorder;
                    var width = logicalResizeBorder.Width;
                    if (x1 <= width)
                    {
                        num1 = 13;
                    }
                    else
                    {
                        var x2 = point2.X;
                        logicalResizeBorder = LogicalResizeBorder;
                        var num2 = 3.0 * logicalResizeBorder.Width;
                        num1 = x2 + num2 < ActualWidth ? 12 : 14;
                    }
                }
            }
            else
            {
                handled = false;
                return IntPtr.Zero;
            }
        }

        handled = true;
        return new IntPtr(num1);
    }

    private void WmSysCommand(IntPtr hWnd, IntPtr wParam, IntPtr lParam)
    {
        var scWparam = (int)wParam & 65520;
        if (scWparam is 61488 or 61472 or 61456 or 61440 && WindowState == WindowState.Normal && !IsAeroSnappedToMonitor(hWnd))
            _logicalSizeForRestore = new Rect(Left, Top, Width, Height);
        if (scWparam != 61728 || WindowState == WindowState.Minimized || _logicalSizeForRestore.Width <= 0.0 ||
            _logicalSizeForRestore.Height <= 0.0)
            return;
        Left = _logicalSizeForRestore.Left;
        Top = _logicalSizeForRestore.Top;
        Width = _logicalSizeForRestore.Width;
        Height = _logicalSizeForRestore.Height;
        _useLogicalSizeForRestore = true;

        bool IsAeroSnappedToMonitor(IntPtr hWnd)
        {
            var monitorInfo = MonitorInfoFromWindow(hWnd);
            var rect = new Rect(Left, Top, Width, Height);
            var deviceRect = hWnd.LogicalToDeviceRect(rect);
            return monitorInfo.RcWork.Height == deviceRect.Height &&
                   monitorInfo.RcWork.Top == deviceRect.Top;
        }
    }

    private void WmWindowPosChanging(IntPtr hwnd, IntPtr lParam)
    {
        var structure = (WindowPosStruct)Marshal.PtrToStructure(lParam, typeof(WindowPosStruct));
        if (((int)structure.flags & 2) != 0 || ((int)structure.flags & 1) != 0 || structure.cx <= 0 ||
            structure.cy <= 0)
            return;
        var floatRect = new Rect(structure.x, structure.y, structure.cx, structure.cy);
        if (_useLogicalSizeForRestore)
        {
            floatRect = hwnd.LogicalToDeviceRect(_logicalSizeForRestore);
            _logicalSizeForRestore = Rect.Empty;
            _useLogicalSizeForRestore = false;
        }

        var rect = _isDragging ? floatRect : DisplayHelper.GetOnScreenPosition(floatRect);
        structure.x = (int)rect.X;
        structure.y = (int)rect.Y;
        structure.cx = (int)rect.Width;
        structure.cy = (int)rect.Height; 
        Marshal.StructureToPtr(structure, lParam, true);
    }

    private void WmWindowPosChanged(IntPtr hWnd, IntPtr lParam)
    {
        try
        {
            if (!DwmOwnsBorder)
            {
                var borderThickness = 0;
                var flag1 = ShouldHaveBorderThickness(hWnd);
                var flag2 = flag1 && (uint)WindowState > 0U;
                if (flag1)
                    borderThickness = (int)(this.LogicalToDeviceUnitsX(1) + 0.5);
                if (flag2)
                    Task.Run(async () =>
                    {
                        await Task.Delay(200);
                        UpdateBorderPlacement(borderThickness);
                    });
                else
                    UpdateBorderPlacement(borderThickness);
            }

            var windowPlacement = User32.GetWindowPlacement(hWnd);
            OnWindowPosChanged(hWnd, windowPlacement.showCmd, windowPlacement.rcNormalPosition.ToInt32Rect());
            UpdateZOrderOfThisAndOwner();
        }
        catch (Win32Exception)
        {
        }

        void UpdateBorderPlacement(int borderThickness)
        {
            User32.GetClientRect(hWnd, out var lpRect);
            User32.MapWindowPoints(hWnd, IntPtr.Zero, ref lpRect, 2);
            LeftBorder?.Resize(lpRect.Left - borderThickness, lpRect.Top, borderThickness,
                lpRect.Height + borderThickness);
            RightBorder?.Resize(lpRect.Right, lpRect.Top, borderThickness, lpRect.Height + borderThickness);
            BottomBorder?.Resize(lpRect.Left, lpRect.Bottom, lpRect.Width, borderThickness);
        }
    }

    protected virtual bool ShouldHaveBorderThickness(IntPtr hWnd)
    {
        return User32.IsWindowVisible(hWnd) && !User32.IsZoomed(hWnd) && !User32.IsIconic(hWnd);
    }

    private void UpdateZOrderOfThisAndOwner()
    {
        if (_updatingZOrder)
            return;
        try
        {
            _updatingZOrder = true;
            if (!(WindowHelper.Owner != IntPtr.Zero))
                return;
            UpdateZOrderOfOwner(WindowHelper.Owner);
        }
        finally
        {
            _updatingZOrder = false;
        }
    }

    private static void UpdateZOrderOfOwner(IntPtr hwndOwner)
    {
        var lastOwnedWindow = IntPtr.Zero;
        User32.EnumThreadWindows(Kernel32.GetCurrentThreadId(), (hwnd, _) =>
        {
            if (User32.GetWindow(hwnd, 4) == hwndOwner)
                lastOwnedWindow = hwnd;
            return true;
        }, IntPtr.Zero);
        if (lastOwnedWindow == IntPtr.Zero || !(User32.GetWindow(hwndOwner, 3) != lastOwnedWindow))
            return;
        User32.SetWindowPos(hwndOwner, lastOwnedWindow, 0, 0, 0, 0, 19);
    }

    protected virtual void OnWindowPosChanged(IntPtr hWnd, int showCmd, Int32Rect rcNormalPosition)
    {
    }

    private static MonitorInfoStruct MonitorInfoFromWindow(IntPtr hWnd)
    {
        var hMonitor = User32.MonitorFromWindow(hWnd, 2);
        var monitorInfo = new MonitorInfoStruct
        {
            CbSize = (uint)Marshal.SizeOf(typeof(MonitorInfoStruct))
        };
        User32.GetMonitorInfo(hMonitor, ref monitorInfo);
        return monitorInfo;
    }

    public static void ShowWindowMenu(HwndSource source, Visual element, Point elementPoint, Size elementSize)
    {
        if (elementPoint.X < 0.0 || elementPoint.X > elementSize.Width || elementPoint.Y < 0.0 ||
            elementPoint.Y > elementSize.Height)
            return;
        var screen = element.PointToScreen(elementPoint);
        var window = element.FindAncestorOrSelf<ShadowChromeWindow>();
        var canMaximize = window?.HasMaximizeButton ?? true;
        var canMinimize = window?.HasMinimizeButton ?? true;
        ShowWindowMenu(source, screen, canMinimize, canMaximize);
    }

    protected static void ShowWindowMenu(HwndSource source, Point screenPoint, bool canMinimize, bool canMaximize)
    {
        var systemMetrics = User32.GetSystemMetrics(40);
        var systemMenu = User32.GetSystemMenu(source.Handle, false);
        var windowPlacement = User32.GetWindowPlacement(source.Handle);
        using (new SuppressRedrawScope(source.Handle))
        {
            var minFalg = canMinimize ? 0U : 1U;
            var maxFalg = canMaximize ? 0U : 1U;
            if (windowPlacement.showCmd == 1)
            {
                User32.EnableMenuItem(systemMenu, 61728U, 1U);
                User32.EnableMenuItem(systemMenu, 61456U, 0U);
                User32.EnableMenuItem(systemMenu, 61440U, 0U);
                User32.EnableMenuItem(systemMenu, 61488U, 0U | maxFalg);
                User32.EnableMenuItem(systemMenu, 61472U, 0U | minFalg);
                User32.EnableMenuItem(systemMenu, 61536U, 0U);
            }
            else if (windowPlacement.showCmd == 3)
            {
                User32.EnableMenuItem(systemMenu, 61728U, 0U);
                User32.EnableMenuItem(systemMenu, 61456U, 1U);
                User32.EnableMenuItem(systemMenu, 61440U, 1U);
                User32.EnableMenuItem(systemMenu, 61488U, 1U | maxFalg);
                User32.EnableMenuItem(systemMenu, 61472U, 0U | minFalg);
                User32.EnableMenuItem(systemMenu, 61536U, 0U);
            }
        }

        var fuFlags = (uint)(systemMetrics | 256 | 128 | 2);
        var num1 = User32.TrackPopupMenuEx(systemMenu, fuFlags,
            (int)screenPoint.X, (int)screenPoint.Y, source.Handle, IntPtr.Zero);
        if (num1 == 0)
            return;
        User32.PostMessage(source.Handle, 274, new IntPtr(num1), IntPtr.Zero);
    }

    private IntPtr WmStyleChanging(IntPtr wParam, IntPtr lParam)
    {
        var structure = Marshal.PtrToStructure<StyleStruct>(lParam);
        if (wParam.ToInt64() == -16L)
        {
            structure.StyleNew |= 13565952;
            if (!IsResizable) 
                structure.StyleNew &= ~262144;
            if (!HasMaximizeButton) 
                structure.StyleNew &= ~65536;
            if (!HasMinimizeButton)
                structure.StyleNew &= ~131072;
        }
        Marshal.StructureToPtr(structure, lParam, true);
        return IntPtr.Zero;
    }

    public void ChangeOwnerForActivate(IntPtr newOwner) => _ownerForActivate = newOwner;

    public void ChangeOwner(IntPtr newOwner)
    {
        WindowHelper.Owner = newOwner;
        UpdateZOrderOfThisAndOwner();
    }

    private class ShadowBorder : Window
    {
        public ShadowBorder(ShadowChromeWindow window)
        {
            Focusable = false;
            Height = 0.0;
            IsHitTestVisible = false;
            Owner = Requires.NotNull(window, nameof(window));
            ResizeMode = ResizeMode.NoResize;
            ShowActivated = false;
            ShowInTaskbar = false;
            Width = 0.0;
            WindowStyle = WindowStyle.None;
            SetBinding(BackgroundProperty, new Binding
            {
                Path = new PropertyPath("BorderBrush", Array.Empty<object>()),
                Source = Owner
            });
        }

        private IntPtr Hwnd { get; set; }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            Hwnd = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(Hwnd)?.AddHook(WndProc);
        }

        public void Resize(int left, int top, int width, int height)
        {
            User32.SetWindowPos(Hwnd, IntPtr.Zero, left, top, 0, 0, 21);
            User32.SetWindowPos(Hwnd, IntPtr.Zero, 0, 0, width, height, 22);
        }

        private IntPtr WndProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (message)
            {
                case 2:
                    var hwndSource = HwndSource.FromHwnd(hwnd);
                    hwndSource?.RemoveHook(WndProc);

                    break;
                case 7:
                    handled = true;
                    User32.SetFocus(wParam);
                    break;
                case 36:
                    Marshal.StructureToPtr(Marshal.PtrToStructure<MinMaxInfoStruct>(lParam) with
                    {
                        PtMinTrackSize = new PointStruct()
                    }, lParam, true);
                    break;
                case 124:
                    var int64 = (GWL)wParam.ToInt64();
                    var structure = Marshal.PtrToStructure<StyleStruct>(lParam);
                    switch (int64)
                    {
                        case GWL.ExStyle:
                            structure.StyleNew = 134217856;
                            break;
                        case GWL.Style:
                            structure.StyleNew = (structure.StyleNew & 268435456) > 0 ? 268435456 : 0;
                            structure.StyleNew |= -2046820352;
                            break;
                    }

                    Marshal.StructureToPtr(structure, lParam, true);
                    break;
                case 132:
                    handled = true;
                    return new IntPtr(-1);
            }

            return IntPtr.Zero;
        }
    }

    private class NonClientButtonManager
    {
        private WindowTitleBarButton? _trackedButton;

        public NonClientButtonManager(Window window) => Owner = Requires.NotNull(window, nameof(window));

        private Window Owner { get; }

        public void ClearTrackedButton()
        {
            if (_trackedButton == null)
                return;
            _trackedButton.IsNCMouseOver = false;
            _trackedButton.IsNCPressed = false;
            _trackedButton = null;
        }

        public bool ClickTrackedButton(IntPtr lParam)
        {
            if (GetButtonUnderMouse(lParam) != _trackedButton || _trackedButton is not { IsNCPressed: true })
                return false;
            _trackedButton.IsNCPressed = false;
            if (_trackedButton.Command is not RoutedCommand command)
                return false;
            command.Execute(_trackedButton.CommandParameter, _trackedButton);
            return true;
        }

        public bool HoverTrackedButton(IntPtr lParam)
        {
            var buttonUnderMouse = GetButtonUnderMouse(lParam);
            if (buttonUnderMouse == _trackedButton)
                return true;
            if (_trackedButton != null)
            {
                _trackedButton.IsNCMouseOver = false;
                _trackedButton.IsNCPressed = false;
            }

            _trackedButton = buttonUnderMouse;
            if (_trackedButton != null)
                _trackedButton.IsNCMouseOver = true;
            return true;
        }

        public bool PressTrackedButton(IntPtr lParam)
        {
            if (GetButtonUnderMouse(lParam) != _trackedButton || _trackedButton == null)
                return false;
            _trackedButton.IsNCPressed = true;
            return true;
        }

        private WindowTitleBarButton? GetButtonUnderMouse(IntPtr lParam)
        {
            return VisualTreeHelper.HitTest(Owner, LogicalPointFromLParam(lParam))?.VisualHit is not Visual
                visualHit
                ? null
                : visualHit.FindAncestorOrSelf<WindowTitleBarButton>();

            Point LogicalPointFromLParam(IntPtr lParam) => Owner.PointFromScreen(
                new Point(User32.GetXLParam(lParam.ToInt32Unchecked()),
                    User32.GetYLParam(lParam.ToInt32Unchecked())));
        }
    }

    private class SuppressRedrawScope : IDisposable
    {
        private readonly IntPtr _hwnd;
        private readonly bool _suppressedRedraw;

        public SuppressRedrawScope(IntPtr hwnd)
        {
            _hwnd = hwnd;
            if ((User32.GetWindowLong(hwnd, GWL.Style) & 268435456) == 0)
                return;
            SetRedraw(false);
            _suppressedRedraw = true;
        }

        public void Dispose()
        {
            if (!_suppressedRedraw)
                return;
            SetRedraw(true);
            const RedrawWindowFlags flags = RedrawWindowFlags.Invalidate | RedrawWindowFlags.AllChildren | RedrawWindowFlags.Frame;
            User32.RedrawWindow(_hwnd, IntPtr.Zero, IntPtr.Zero, flags);
        }

        private void SetRedraw(bool state) => User32.SendMessage(_hwnd, 11, new IntPtr(Convert.ToInt32(state)));
    }
}