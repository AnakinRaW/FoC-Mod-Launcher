using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using AnakinRaW.CommonUtilities.Wpf.DPI;
using AnakinRaW.CommonUtilities.Wpf.Input;
using AnakinRaW.CommonUtilities.Wpf.NativeMethods;
using AnakinRaW.CommonUtilities.Wpf.Utilities;
using Validation;
using Vanara.PInvoke;

namespace AnakinRaW.CommonUtilities.Wpf.Controls;

public class WindowBase : Window
{
    private Rect _logicalSizeForRestore = Rect.Empty;
    private bool _useLogicalSizeForRestore;
    private bool _isDragging;
    private bool _updatingZOrder;
    private IntPtr _ownerForActivate;

    private static uint _notifyOwnerActivate;

    private protected HwndSource? HwndSource;

    public static readonly DependencyProperty HasMaximizeButtonProperty = DependencyProperty.Register(
        nameof(HasMaximizeButton), typeof(bool), typeof(WindowBase),
        new FrameworkPropertyMetadata(Boxes.BooleanTrue, OnWindowStyleChanged));

    public static readonly DependencyProperty HasMinimizeButtonProperty = DependencyProperty.Register(
        nameof(HasMinimizeButton), typeof(bool), typeof(WindowBase),
        new FrameworkPropertyMetadata(Boxes.BooleanTrue, OnWindowStyleChanged));

    public static readonly DependencyProperty IsResizableProperty = DependencyProperty.Register(
        nameof(IsResizable), typeof(bool), typeof(WindowBase),
        new PropertyMetadata(Boxes.BooleanTrue, OnResizeChanged));

    public bool IsResizable
    {
        get => (bool)GetValue(IsResizableProperty);
        set => SetValue(IsResizableProperty, value);
    }

    public bool HasMaximizeButton
    {
        get => (bool)GetValue(HasMaximizeButtonProperty);
        set => SetValue(HasMaximizeButtonProperty, Boxes.Box(value));
    }

    public bool HasMinimizeButton
    {
        get => (bool)GetValue(HasMinimizeButtonProperty);
        set => SetValue(HasMinimizeButtonProperty, Boxes.Box(value));
    }
    
    public IWindowViewModel ViewModel { get; protected set; }

    protected WindowInteropHelper WindowHelper { get; }

    private static int PressedMouseButtons
    {
        get
        {
            var pressedMouseButtons = 0;
            if (IsKeyPressed(1))
                pressedMouseButtons |= 1;
            if (IsKeyPressed(2))
                pressedMouseButtons |= 2;
            if (IsKeyPressed(4))
                pressedMouseButtons |= 16;
            if (IsKeyPressed(5))
                pressedMouseButtons |= 32;
            if (IsKeyPressed(6))
                pressedMouseButtons |= 64;
            return pressedMouseButtons;
        }
    }

    private static uint NotifyOwnerActive
    {
        get
        {
            if (_notifyOwnerActivate == 0)
                _notifyOwnerActivate = User32.RegisterWindowMessage("NotifyOwnerActive{A982313C-756C-4da9-8BD0-0C375A45784B}");
            return _notifyOwnerActivate;
        }
    }

    static WindowBase()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowBase),
            new FrameworkPropertyMetadata(typeof(WindowBase)));
        CommandManager.RegisterClassCommandBinding(typeof(UIElement),
            new CommandBinding(ViewCommands.ToggleMaximizeRestoreWindow, OnToggleMaximizeRestoreWindow));
        CommandManager.RegisterClassCommandBinding(typeof(UIElement),
            new CommandBinding(ViewCommands.MinimizeWindow, OnMinimizeWindow));
        CommandManager.RegisterClassCommandBinding(typeof(UIElement),
            new CommandBinding(ViewCommands.CloseWindow, OnCloseWindow));
    }

    public WindowBase(IWindowViewModel viewModel)
    {
        Requires.NotNull(viewModel, nameof(viewModel));
        DataContext = viewModel;
        viewModel.CloseDialogRequest += OnCloseRequested;
        ViewModel = viewModel;
        WindowHelper = new WindowInteropHelper(this);
        DataContextChanged += OnDataContextChanged;
    }

    public void ChangeOwner(IntPtr newOwner)
    {
        WindowHelper.Owner = newOwner;
        UpdateZOrderOfThisAndOwner();
    }

    public void ChangeOwnerForActivate(IntPtr newOwner)
    {
        _ownerForActivate = newOwner;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        HwndSource = (HwndSource)PresentationSource.FromVisual(this);
        HwndSource.AddHook(WndProcHook);
        UpdateWindowStyle();
        base.OnSourceInitialized(e);
        SetWindowIcon();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (HwndSource != null)
        {
            HwndSource.Dispose();
            HwndSource = null;
        }
        base.OnClosed(e);
    }

    protected virtual void UpdateWindowStyle()
    {
        if (HwndSource == null)
            return;
        var handle = HwndSource.Handle;
        if (handle == IntPtr.Zero)
            return;
        var currentStyle = (User32.WindowStyles) User32.GetWindowLong(handle, User32.WindowLongFlags.GWL_STYLE);
        var newStyle = !HasMaximizeButton ? currentStyle & ~User32.WindowStyles.WS_TABSTOP : currentStyle | User32.WindowStyles.WS_TABSTOP;
        newStyle = !HasMinimizeButton ? newStyle & ~User32.WindowStyles.WS_GROUP : newStyle | User32.WindowStyles.WS_GROUP;
        User32.SetWindowLong(handle, User32.WindowLongFlags.GWL_STYLE, (int)newStyle);
    }

    protected virtual IntPtr WndProcHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case 2:
                var hwndSource = HwndSource.FromHwnd(hwnd);
                hwndSource?.RemoveHook(WndProcHook);
                break;
            case 6:
                WmActivate(hwnd, wParam, lParam);
                break;
            case 26 when wParam.ToInt32() == 67:
            case 21:
                OnThemeChanged();
                handled = true;
                break;
            case 70:
                WmWindowPosChanging(hwnd, lParam);
                break;
            case 124:
                return WmStyleChanging(wParam, lParam);
            case 164:
            case 165:
            case 166:
                RaiseNonClientMouseMessageAsClient(hwnd, msg, wParam, lParam);
                handled = true;
                break;
            case 274:
                WmSysCommand(hwnd, wParam, lParam);
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

    protected virtual void OnThemeChanged()
    {
    }

    protected void EnsureOnScreen()
    {
        var deviceRect = this.LogicalToDeviceRect();
        var displayForWindowRect = DisplayHelper.FindDisplayForWindowRect(deviceRect);
        var onScreenPosition = DisplayHelper.GetOnScreenPosition(deviceRect, displayForWindowRect, true);
        User32.SetWindowPos(WindowHelper.Handle, IntPtr.Zero, (int)onScreenPosition.Left,
            (int)onScreenPosition.Top, (int)onScreenPosition.Width, (int)onScreenPosition.Height,
            User32.SetWindowPosFlags.SWP_NOACTIVATE | User32.SetWindowPosFlags.SWP_NOZORDER);
    }

    protected T? GetTemplateChild<T>(string name) where T : class
    {
        var templateChild = GetTemplateChild(name) as T;
        try
        {
        }
        catch (InvalidOperationException)
        {
        }
        return templateChild;
    }

    private protected void UpdateZOrderOfThisAndOwner()
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

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (ViewModel is not null)
            ViewModel.CloseDialogRequest -= OnCloseRequested;
        if (e.NewValue is not IWindowViewModel windowViewModel)
            return;
        ViewModel = windowViewModel;
        windowViewModel.CloseDialogRequest += OnCloseRequested;
    }

    private static void OnCloseWindow(object sender, ExecutedRoutedEventArgs args)
    {
        if (args.Parameter is not Window window)
            return;
        window.Close();
    }

    private static void OnMinimizeWindow(object sender, ExecutedRoutedEventArgs args)
    {
        if (args.Parameter is not Window window)
            return;
        window.WindowState = WindowState.Minimized;
    }

    private static void OnToggleMaximizeRestoreWindow(object sender, ExecutedRoutedEventArgs args)
    {
        if (args.Parameter is not Window window)
            return;
        var handle = new WindowInteropHelper(window).Handle;
        User32.SendMessage(handle, 274,
            window.WindowState == WindowState.Maximized ? new IntPtr(61728) : new IntPtr(61488), IntPtr.Zero);
    }

    private void OnCloseRequested(object? sender, EventArgs e)
    {
        Dispatcher.Invoke(DispatcherPriority.Input, Close);
    }

    private static void OnResizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not WindowBase windowBase)
            return;
        windowBase.UpdateResizable((bool)e.NewValue);
    }

    private static void OnWindowStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        if (obj is not WindowBase windowBase)
            return;
        windowBase.UpdateWindowStyle();
    }

    private void UpdateResizable(bool resizable)
    {
        if (HwndSource == null)
            return;
        var handle = HwndSource.Handle;
        if (handle == IntPtr.Zero)
            return;
        var newStyle = User32.GetWindowLong(handle, User32.WindowLongFlags.GWL_STYLE);
        if (resizable)
            newStyle |= 262144;
        else
            newStyle &= ~262144;
        User32.SetWindowLong(handle, User32.WindowLongFlags.GWL_STYLE, newStyle);
    }

    private void WmWindowPosChanging(IntPtr hwnd, IntPtr lParam)
    {
        var structure = (User32.WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(User32.WINDOWPOS));
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
            return monitorInfo.rcWork.Height == deviceRect.Height &&
                   monitorInfo.rcWork.Top == deviceRect.Top;
        }
    }

    private static User32.MONITORINFO MonitorInfoFromWindow(IntPtr hWnd)
    {
        var hMonitor = User32.MonitorFromWindow(hWnd, User32.MonitorFlags.MONITOR_DEFAULTTONEAREST);
        var monitorInfo = new User32.MONITORINFO
        {
            cbSize = (uint)Marshal.SizeOf(typeof(User32.MONITORINFO))
        };
        User32.GetMonitorInfo(hMonitor, ref monitorInfo);
        return monitorInfo;
    }

    private void WmActivate(IntPtr hwnd, IntPtr wParam, IntPtr lParam)
    {
        if (_ownerForActivate != IntPtr.Zero)
            User32.SendMessage(_ownerForActivate, NotifyOwnerActive, wParam, lParam);
    }

    private static void UpdateZOrderOfOwner(IntPtr hwndOwner)
    {
        var lastOwnedWindow = IntPtr.Zero;
        User32.EnumThreadWindows(Kernel32.GetCurrentThreadId(), (hwnd, _) =>
        {
            if (User32.GetWindow(hwnd, 4) == hwndOwner)
                lastOwnedWindow = hwnd.DangerousGetHandle();
            return true;
        }, IntPtr.Zero);
        if (lastOwnedWindow == IntPtr.Zero || !(User32.GetWindow(hwndOwner, 3) != lastOwnedWindow))
            return;
        User32.SetWindowPos(hwndOwner, lastOwnedWindow, 0, 0, 0, 0,
            User32.SetWindowPosFlags.SWP_NOACTIVATE | User32.SetWindowPosFlags.SWP_NOMOVE |
            User32.SetWindowPosFlags.SWP_NOSIZE);
    }

    private static void RaiseNonClientMouseMessageAsClient(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
    {
        var point = new POINT
        {
            X = NativeExtensions.GetXLParam(lParam.ToInt32Unchecked()),
            Y = NativeExtensions.GetYLParam(lParam.ToInt32Unchecked())
        };
        User32.ScreenToClient(hWnd, ref point);
        User32.SendMessage(hWnd, msg + 513 - 161, new IntPtr(PressedMouseButtons),
            NativeExtensions.MakeParam(point.X, point.Y));
    }

    private void SetWindowIcon()
    {
        if (ViewModel.ShowIcon)
            IconHelper.UseWindowIconAsync(windowIcon => Icon = windowIcon);
    }

    private static bool IsKeyPressed(int vKey)
    {
        return User32.GetKeyState(vKey) < 0;
    }
}