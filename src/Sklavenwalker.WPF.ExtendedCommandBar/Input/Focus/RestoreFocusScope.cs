using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Sklavenwalker.CommonUtilities.Wpf.Utils;
using Sklavenwalker.Wpf.CommandBar.NativeMethods;

namespace Sklavenwalker.Wpf.CommandBar.Input.Focus;

internal abstract class RestoreFocusScope
{
    private List<DependencyObject>? _focusAncestors;

    protected RestoreFocusScope(IInputElement restoreFocus, IntPtr restoreFocusWindow)
    {
        RestoreFocusWindow = restoreFocusWindow;
        RestoreFocus = restoreFocus;
    }

    internal static bool IsActivationSynchronizationEnabledForControl(IInputElement focusableItem)
    {
        return focusableItem is not UIElement element || FocusHelper.GetEnableActivationSynchronization(element);
    }

    protected abstract bool IsRestorationTargetValid();

    public virtual void PerformRestoration()
    {
        if (!IsRestorationTargetValid())
            return;
        if (RestoreFocusWindow != IntPtr.Zero)
        {
            User32.SetFocus(RestoreFocusWindow);
        }
        else
        {
            if (RestoreFocus == null || !IsActivationSynchronizationEnabledForControl(RestoreFocus))
                return;
            Keyboard.Focus(RestoreFocus);
        }
    }

    protected IInputElement? RestoreFocus
    {
        get
        {
            IInputElement? restoreFocus = null;
            if (_focusAncestors != null)
                restoreFocus = _focusAncestors.Find(e => e.IsConnectedToPresentationSource()) as IInputElement;
            return restoreFocus;
        }
        private set
        {
            _focusAncestors = new List<DependencyObject>();
            for (var sourceElement = value as DependencyObject; sourceElement != null; sourceElement = sourceElement.GetVisualOrLogicalParent())
            {
                if (sourceElement is IInputElement inputElement && inputElement.Focusable)
                    _focusAncestors.Add(sourceElement);
            }
        }
    }

    protected IntPtr RestoreFocusWindow { get; private set; }
}