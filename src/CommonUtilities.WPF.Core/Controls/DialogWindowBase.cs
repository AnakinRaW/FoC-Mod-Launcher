using System;
using System.Windows;
using System.Windows.Interop;
using Sklavenwalker.CommonUtilities.Wpf.NativeMethods;
using Sklavenwalker.CommonUtilities.Wpf.Utilities;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public abstract class DialogWindowBase : Window
{
    private HwndSource? _hwndSource;

    public static readonly DependencyProperty HasMaximizeButtonProperty = DependencyProperty.Register(
        nameof(HasMaximizeButton), typeof(bool), typeof(DialogWindowBase),
        new FrameworkPropertyMetadata(Boxes.BooleanFalse, OnWindowStyleChanged));

    public static readonly DependencyProperty HasMinimizeButtonProperty = DependencyProperty.Register(
        nameof(HasMinimizeButton), typeof(bool), typeof(DialogWindowBase),
        new FrameworkPropertyMetadata(Boxes.BooleanFalse, OnWindowStyleChanged));

    public static readonly DependencyProperty HasDialogFrameProperty = DependencyProperty.Register(
        nameof(HasDialogFrame), typeof(bool), typeof(DialogWindowBase),
        new FrameworkPropertyMetadata(Boxes.BooleanTrue, OnWindowStyleChanged));

    public static readonly DependencyProperty IsCloseButtonEnabledProperty =
        DependencyProperty.Register(nameof(IsCloseButtonEnabled), typeof(bool), typeof(DialogWindowBase),
            new PropertyMetadata(Boxes.BooleanTrue, OnWindowStyleChanged));

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

    static DialogWindowBase()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogWindowBase), new FrameworkPropertyMetadata(typeof(DialogWindowBase)));
    }

    protected DialogWindowBase()
    {
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }

    public bool? ShowModal()
    {
        return WindowHelper.ShowModal(this);
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
        ((DialogWindowBase)obj).UpdateWindowStyle();
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

    protected virtual IntPtr WndProcHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == 26 && wParam.ToInt32() == 67 || msg == 21)
        {
            OnDialogThemeChanged();
            handled = true;
        }
        return IntPtr.Zero;
    }

    protected virtual void OnDialogThemeChanged()
    {
    }
}