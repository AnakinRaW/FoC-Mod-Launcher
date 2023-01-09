using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using AnakinRaW.CommonUtilities.Wpf.DPI;
using AnakinRaW.CommonUtilities.Wpf.NativeMethods;
using AnakinRaW.CommonUtilities.Wpf.Utilities;
using Validation;

namespace AnakinRaW.CommonUtilities.Wpf.Controls;

public class ShadowChromeWindow : WindowBase
{
    public static readonly Size LogicalResizeBorder = new(6.0, 6.0);

    public static readonly DependencyProperty ActiveGlowColorProperty = DependencyProperty.Register(
        nameof(ActiveGlowColor), typeof(Color), typeof(ShadowChromeWindow),
        new FrameworkPropertyMetadata(Colors.Transparent, OnGlowColorChanged));

    public static readonly DependencyProperty InactiveGlowColorProperty = DependencyProperty.Register(
        nameof(InactiveGlowColor), typeof(Color), typeof(ShadowChromeWindow),
        new FrameworkPropertyMetadata(Colors.Transparent, OnGlowColorChanged));

    private static readonly DependencyPropertyKey DwmOwnsBorderPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(DwmOwnsBorder), typeof(bool), typeof(ShadowChromeWindow),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender, DwmOwnsBorderChanged));
    

    public static readonly DependencyProperty DwmOwnsBorderProperty =
        DwmOwnsBorderPropertyKey.DependencyProperty;
    
    private NonClientButtonManager NCButtonManager { get; }

    private ShadowBorder? LeftBorder { get; set; }

    private ShadowBorder? RightBorder { get; set; }

    private ShadowBorder? BottomBorder { get; set; }

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

    public ShadowChromeWindow(IWindowViewModel viewModel) : base(viewModel)
    {
        NCButtonManager = new NonClientButtonManager(this);
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

    protected virtual void OnWindowPosChanged(IntPtr hWnd, int showCmd, Int32Rect rcNormalPosition)
    {
    }

    protected virtual bool ShouldHaveBorderThickness(IntPtr hWnd)
    {
        return User32.IsWindowVisible(hWnd) && !User32.IsZoomed(hWnd) && !User32.IsIconic(hWnd);
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

    protected override IntPtr WndProcHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        base.WndProcHook(hwnd, msg, wParam, lParam, ref handled);
        switch (msg)
        {
            case 6:
                WmActivate(hwnd, wParam, lParam);
                break;
            case 12:
            case 128:
                return CallDefWindowProcWithoutRedraw(hwnd, msg, wParam, lParam, ref handled);
            case 71:
                WmWindowPosChanged(hwnd, lParam);
                break;
            case 131:
                return WmNcCalcSize(hwnd, wParam, lParam, ref handled);
            case 132:
                return WmNcHitTest(hwnd, lParam, ref handled);
            case 160:
                handled = NCButtonManager.HoverTrackedButton(lParam);
                break;
            case 161:
                handled = NCButtonManager.PressTrackedButton(lParam);
                break;
            case 162:
                handled = NCButtonManager.ClickTrackedButton(lParam);
                break;
            case 512:
            case 674:
            case 675:
                NCButtonManager.ClearTrackedButton();
                break;
        }

        return IntPtr.Zero;
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

    private class ShadowBorder : Window
    {
        private IntPtr Hwnd { get; set; }

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

        public void Resize(int left, int top, int width, int height)
        {
            User32.SetWindowPos(Hwnd, IntPtr.Zero, left, top, 0, 0, 21);
            User32.SetWindowPos(Hwnd, IntPtr.Zero, 0, 0, width, height, 22);
        }


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            Hwnd = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(Hwnd)?.AddHook(WndProc);
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