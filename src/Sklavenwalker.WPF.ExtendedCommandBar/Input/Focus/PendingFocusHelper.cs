using System;
using System.Windows;
using Sklavenwalker.CommonUtilities.Wpf.Utils;
using Validation;

namespace Sklavenwalker.Wpf.CommandBar.Input.Focus;

internal static class PendingFocusHelper
{
    private static FrameworkElement? _pendingFocusElement;

    private static Action<FrameworkElement>? PendingFocusAction { get; set; }

    private static FrameworkElement? PendingFocusElement
    {
        get => _pendingFocusElement;
        set
        {
            if (_pendingFocusElement == value)
                return;
            if (_pendingFocusElement != null)
                _pendingFocusElement.Loaded -= OnPendingFocusElementLoaded;
            _pendingFocusElement = value;
            if (_pendingFocusElement == null)
                return;
            _pendingFocusElement.Loaded += OnPendingFocusElementLoaded;
        }
    }

    private static void OnPendingFocusElementLoaded(object sender, RoutedEventArgs args)
    {
        var pendingFocusElement = PendingFocusElement;
        if (pendingFocusElement != null)
            MoveFocusInto(pendingFocusElement, PendingFocusAction);
        PendingFocusElement = null;
        PendingFocusAction = null;
    }

    private static void MoveFocusInto(FrameworkElement element, Action<FrameworkElement>? focusAction)
    {
        if (focusAction != null)
            focusAction(element);
        else
            FocusHelper.MoveFocusInto(element);
    }

    public static void SetFocusOnLoad(FrameworkElement element, Action<FrameworkElement>? focusAction = null)
    {
        Requires.NotNull((object)element, nameof(element));
        if (element.IsLoaded && element.IsConnectedToPresentationSource())
        {
            PendingFocusElement = null;
            PendingFocusAction = null;
            MoveFocusInto(element, focusAction);
        }
        else
        {
            PendingFocusElement = element;
            PendingFocusAction = focusAction;
        }
    }
}