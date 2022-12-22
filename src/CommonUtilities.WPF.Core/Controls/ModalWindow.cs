using System;
using System.Windows;
using System.Windows.Interop;
using Sklavenwalker.CommonUtilities.Wpf.NativeMethods;
using Sklavenwalker.CommonUtilities.Wpf.Utilities;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public class ModalWindow : Window
{
    private HwndSource? _hwndSource;

    public static readonly DependencyProperty HasMaximizeButtonProperty = DependencyProperty.Register(
        nameof(HasMaximizeButton), typeof(bool), typeof(ModalWindow),
        new FrameworkPropertyMetadata(Boxes.BooleanFalse, OnWindowStyleChanged));

    public static readonly DependencyProperty HasMinimizeButtonProperty = DependencyProperty.Register(
        nameof(HasMinimizeButton), typeof(bool), typeof(ModalWindow),
        new FrameworkPropertyMetadata(Boxes.BooleanFalse, OnWindowStyleChanged));

    public static readonly DependencyProperty HasDialogFrameProperty = DependencyProperty.Register(
        nameof(HasDialogFrame), typeof(bool), typeof(ModalWindow),
        new FrameworkPropertyMetadata(Boxes.BooleanTrue, OnWindowStyleChanged));

    public static readonly DependencyProperty IsResizableProperty = DependencyProperty.Register(
        nameof(IsResizable), typeof(bool), typeof(ModalWindow),
        new PropertyMetadata(true, OnResizeChanged));

    public static readonly DependencyProperty IsCloseButtonEnabledProperty =
        DependencyProperty.Register(nameof(IsCloseButtonEnabled), typeof(bool), typeof(ModalWindow),
            new PropertyMetadata(Boxes.BooleanTrue, OnWindowStyleChanged));

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

    public bool HasDialogFrame
    {
        get => (bool)GetValue(HasDialogFrameProperty);
        set => SetValue(HasDialogFrameProperty, Boxes.Box(value));
    }

    public bool IsCloseButtonEnabled
    {
        get => (bool)GetValue(IsCloseButtonEnabledProperty);
        set => SetValue(IsCloseButtonEnabledProperty, value);
    }

    static ModalWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ModalWindow), new FrameworkPropertyMetadata(typeof(ModalWindow)));
    }

    public ModalWindow()
    {
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }

    public bool? ShowModal()
    {
        return WindowHelper.ShowModal(this);
    }

    public void EnableOwner()
    {
        if (Owner is null)
            return;
        Owner.IsEnabled = true;
        if (Owner.IsActive)
            return;
        Owner.Activate();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
        _hwndSource.AddHook(WndProcHook);
        UpdateWindowStyle();
        base.OnSourceInitialized(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_hwndSource != null)
        {
            _hwndSource.Dispose();
            _hwndSource = null;
        }
        base.OnClosed(e);
    }

    private static void OnWindowStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        ((ModalWindow)obj).UpdateWindowStyle();
    }

    private static void OnResizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var window = (Window)d;
        if (PresentationSource.FromVisual(window) is not HwndSource hwndSource)
            return;
        UpdateResizable(hwndSource.Handle, (bool)e.NewValue);
    }
    
    private void UpdateWindowStyle()
    {
        if (_hwndSource == null)
            return;
        var handle = _hwndSource.Handle;
        if (handle == IntPtr.Zero)
            return;
        var currentStyle = User32.GetWindowLong(handle, -16);
        var newStyle = !HasMaximizeButton ? currentStyle & -65537 : currentStyle | 65536;
        newStyle = !HasMinimizeButton ? newStyle & -131073 : newStyle | 131072;
        User32.SetWindowLong(handle, -16, newStyle);
        var extendedStyle = User32.GetWindowLong(handle, -20);
        var newExStyle = !HasDialogFrame ? extendedStyle & -2 : extendedStyle | 1;
        User32.SetWindowLong(handle, -20, newExStyle);
        User32.SendMessage(handle, 128, new IntPtr(1), IntPtr.Zero);
        User32.SendMessage(handle, 128, new IntPtr(0), IntPtr.Zero);
        var systemMenu = User32.GetSystemMenu(handle, false);
        if (systemMenu != IntPtr.Zero)
        {
            var num5 = IsCloseButtonEnabled ? 0U : 1U;
            User32.EnableMenuItem(systemMenu, 61536U, 0U | num5);
        }
        User32.SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, 35);
    }

    private static void UpdateResizable(IntPtr hwnd, bool resizable)
    {
        var newStyle = User32.GetWindowLong(hwnd, -16);
        if (resizable)
            newStyle |= 262144;
        else
            newStyle &= ~262144;
        User32.SetWindowLong(hwnd, -16, newStyle);
    }

    protected virtual IntPtr WndProcHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == 26 && wParam.ToInt32() == 67 || msg == 21)
        {
            OnThemeChanged();
            handled = true;
        }
        return IntPtr.Zero;
    }

    protected virtual void OnThemeChanged()
    {
    }
}