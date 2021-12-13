using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Sklavenwalker.CommonUtilities.Wpf.Utils;
using Sklavenwalker.Wpf.CommandBar.NativeMethods;
using Validation;

namespace Sklavenwalker.Wpf.CommandBar.Input.Focus;

public static class FocusHelper
{
    public static readonly DependencyProperty FocusTargetProperty = DependencyProperty.RegisterAttached("FocusTarget",
        typeof(UIElement), typeof(FocusHelper), new FrameworkPropertyMetadata(null, OnFocusTargetChanged));

    public static readonly DependencyProperty EnableActivationSynchronizationProperty =
        DependencyProperty.RegisterAttached("EnableActivationSynchronization", typeof(bool), typeof(FocusHelper),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));

    public static UIElement GetFocusTarget(UIElement element)
    {
        Requires.NotNull((object)element, nameof(element));
        return (UIElement)element.GetValue(FocusTargetProperty);
    }

    public static void SetFocusTarget(UIElement element, UIElement value)
    {
        Requires.NotNull((object)element, nameof(element));
        element.SetValue(FocusTargetProperty, value);
    }

    public static bool GetEnableActivationSynchronization(UIElement element)
    {
        Requires.NotNull((object)element, nameof(element));
        return (bool)element.GetValue(EnableActivationSynchronizationProperty);
    }

    public static void SetEnableActivationSynchronization(UIElement element, bool value)
    {
        Requires.NotNull((object)element, nameof(element));
        element.SetValue(EnableActivationSynchronizationProperty, value);
    }

    public static void FocusPossiblyUnloadedElement(FrameworkElement element)
    {
        Requires.NotNull((object)element, nameof(element));
        PendingFocusHelper.SetFocusOnLoad(element);
    }

    public static void MoveFocusInto(UIElement element)
    {
        if (IsKeyboardFocusWithin(element))
            return;
        element.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
    }

    public static bool IsKeyboardFocusWithin(UIElement element)
    {
        if (element.IsKeyboardFocusWithin)
            return true;
        var focus = User32.GetFocus();
        if (element.FindDescendants<HwndHost>().Any(descendant => IsChildOrSame(descendant.Handle, focus)))
            return true;
        return element is HwndHost hwndHost && IsKeyboardFocusWithin(hwndHost.Handle);
    }

    public static bool IsKeyboardFocusWithin(IntPtr hwnd)
    {
        return IsChildOrSame(hwnd, User32.GetFocus());
    }

    private static bool IsChildOrSame(IntPtr hwndParent, IntPtr hwndChild)
    {
        return hwndParent == hwndChild || User32.IsChild(hwndParent, hwndChild);
    }

    private static void OnFocusTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var source = (UIElement)d;
        if (e.NewValue == null)
        {
            if (e.OldValue == null)
                return;
            WeakEventManager<UIElement, KeyboardFocusChangedEventArgs>.RemoveHandler(source, "GotKeyboardFocus", OnGotKeyboardFocus);
        }
        else
        {
            if (e.OldValue != null)
                return;
            WeakEventManager<UIElement, KeyboardFocusChangedEventArgs>.AddHandler(source, "GotKeyboardFocus", OnGotKeyboardFocus);
        }
    }

    private static void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        var element = (UIElement)sender;
        if (e.OriginalSource != element)
            return;
        MoveFocusInto(GetFocusTarget(element));
    }
}