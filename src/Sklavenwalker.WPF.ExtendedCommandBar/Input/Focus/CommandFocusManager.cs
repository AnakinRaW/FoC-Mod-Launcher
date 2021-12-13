using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Sklavenwalker.CommonUtilities.Wpf.Utils;
using Sklavenwalker.Wpf.CommandBar.Controls;
using Sklavenwalker.Wpf.CommandBar.Utilities;
using ExtensionMethods = Sklavenwalker.CommonUtilities.Wpf.Utils.ExtensionMethods;


namespace Sklavenwalker.Wpf.CommandBar.Input.Focus;

internal class CommandFocusManager
{
    private static RestoreFocusScope? _restoreFocusScope;
    private static PresentationSource? _currentMenuModeSource;
    private static readonly List<Type> RegisteredCommandFocusElementTypes = new();
    private static bool _isChecking;

    private static PresentationSource? CurrentMenuModeSource
    {
        set
        {
            if (_currentMenuModeSource == value)
                return;
            var currentMenuModeSource = _currentMenuModeSource;
            _currentMenuModeSource = value;
            try
            {
                if (_currentMenuModeSource == null)
                    return;
                InputManager.Current.PushMenuMode(_currentMenuModeSource);
            }
            finally
            {
                if (currentMenuModeSource != null)
                    InputManager.Current.PopMenuMode(currentMenuModeSource);
            }
        }
    }

    public static void Initialize()
    {
        CommandNavigationHelper.CommandFocusModePropertyChanged += OnCommandFocusModePropertyChanged;
        HwndSource.DefaultAcquireHwndFocusInMenuMode = false; 
        // TODO: Add Menu and Toolbar
        RegisterClassHandlers(typeof(ThemedContextMenu));
        EventManager.RegisterClassHandler(typeof(UIElement), ContextMenu.ClosedEvent, new RoutedEventHandler(OnContextMenuClosed));
        InputManager.Current.LeaveMenuMode += (_, _) => CorrectDetachedHwndFocus();
    }

    public static void CancelRestoreFocus()
    {
        _restoreFocusScope = null;
    }

    internal static bool IsRegisteredCommandFocusElement(DependencyObject element)
    {
        switch (element)
        {
            //case VsMenu _:
            case ThemedContextMenu _:
                //case VsToolBar _:
                return true;
            default:
                return (uint)CommandNavigationHelper.GetCommandFocusMode(element) > 0U;
        }
    }

    internal static bool IsInsideAttachedCommandFocusElement(IInputElement? inputElement)
    {
        if (!(inputElement is DependencyObject dependencyObject))
            return false;
        var parentEvaluator = ExtensionMethods.GetVisualOrLogicalParent;
        return dependencyObject.FindAncestorOrSelf(parentEvaluator, IsAttachedCommandFocusElement) != null;
    }

    internal static bool IsInsideCommandContainer(IInputElement inputElement)
    {
        if (!(inputElement is DependencyObject dependencyObject))
            return false;
        var parentEvaluator = ExtensionMethods.GetVisualOrLogicalParent;
        return dependencyObject.FindAncestorOrSelf(parentEvaluator, IsRegisteredCommandFocusElement) != null;
    }

    private static void RegisterClassHandlers(Type type)
    {
        EventManager.RegisterClassHandler(type, Keyboard.PreviewKeyboardInputProviderAcquireFocusEvent,
            new KeyboardInputProviderAcquireFocusEventHandler(OnKeyboardInputProviderAcquireFocus), true);
        EventManager.RegisterClassHandler(type, Keyboard.KeyboardInputProviderAcquireFocusEvent,
            new KeyboardInputProviderAcquireFocusEventHandler(OnKeyboardInputProviderAcquireFocus), true);
        EventManager.RegisterClassHandler(type, Keyboard.PreviewGotKeyboardFocusEvent,
            new KeyboardFocusChangedEventHandler(OnPreviewGotKeyboardFocus), true);
        EventManager.RegisterClassHandler(type, Keyboard.PreviewLostKeyboardFocusEvent,
            new KeyboardFocusChangedEventHandler(OnPreviewLostKeyboardFocus), true);
        EventManager.RegisterClassHandler(type, Keyboard.LostKeyboardFocusEvent,
            new KeyboardFocusChangedEventHandler(OnLostKeyboardFocus), true);
    }

    private static void OnCommandFocusModePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        var newValue = (CommandNavigationHelper.CommandFocusMode)e.NewValue;
        var type = obj.GetType();
        if (newValue == CommandNavigationHelper.CommandFocusMode.None ||
            RegisteredCommandFocusElementTypes.Contains(type) ||
            //type.IsEquivalentTo(typeof(VsMenu)) || type.IsSubclassOf(typeof(VsMenu)) ||
            //type.IsEquivalentTo(typeof(VsToolBar)) || type.IsSubclassOf(typeof(VsToolBar)) || 
            type.IsEquivalentTo(typeof(ThemedContextMenu)) || type.IsSubclassOf(typeof(ThemedContextMenu)))
            return;
        foreach (var focusElementType in RegisteredCommandFocusElementTypes)
        {
            if (focusElementType.IsSubclassOf(type) || type.IsSubclassOf(focusElementType))
                return;
        }

        RegisteredCommandFocusElementTypes.Add(type);
        RegisterClassHandlers(type);
    }

    private static void OnContextMenuClosed(object sender, RoutedEventArgs e)
    {
        if (!IsCurrentThreadMainUIThread() || _restoreFocusScope == null ||
            !IsCommandContainerLosingFocus(e.Source as IInputElement, Keyboard.FocusedElement))
            return;
        CurrentMenuModeSource = null;
        var restoreFocusScope = _restoreFocusScope;
        _restoreFocusScope = null;
        restoreFocusScope.PerformRestoration();
    }
    
    private static void CorrectDetachedHwndFocus()
    {
        if (!(Keyboard.FocusedElement is DependencyObject focusedElement))
            return;
        focusedElement.AcquireWin32Focus(out _);
    }

    private static void OnKeyboardInputProviderAcquireFocus(object sender, KeyboardInputProviderAcquireFocusEventArgs e)
    {
        if (!IsCurrentThreadMainUIThread() || !(sender is DependencyObject dependencyObject) || !IsRegisteredCommandFocusElement(dependencyObject))
            return;
        if (dependencyObject is IInputElement { IsKeyboardFocusWithin: false })
        {
            if (e.RoutedEvent == Keyboard.PreviewKeyboardInputProviderAcquireFocusEvent)
            {
                if (!IsAttachedCommandFocusElement(dependencyObject))
                    CurrentMenuModeSource = PresentationSource.FromDependencyObject(dependencyObject);
            }
            else if (!e.FocusAcquired)
                CurrentMenuModeSource = null;
        }
        if (PresentationSource.FromDependencyObject(dependencyObject) != null)
            return;
        e.Handled = true;
    }

    private static void OnPreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs args)
    {
        if (!IsCurrentThreadMainUIThread() || sender is not DependencyObject dependencyObject || !IsRegisteredCommandFocusElement(dependencyObject))
            return;
        if (_restoreFocusScope == null && IsCommandContainerGainingFocus(args.OldFocus, args.NewFocus))
        {
            if (!IsAttachedCommandFocusElement(dependencyObject))
                CurrentMenuModeSource = PresentationSource.FromDependencyObject(dependencyObject);
            _restoreFocusScope = new CommandRestoreFocusScope(args.OldFocus);
            PreventFocusScopeCommandRedirection(args.NewFocus as DependencyObject);
        }
        if (PresentationSource.FromDependencyObject(dependencyObject) != null)
            return;
        args.Handled = true;
    }

    private static void OnPreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs args)
    {
        if (!IsCurrentThreadMainUIThread() || !(sender is DependencyObject element) || !IsRegisteredCommandFocusElement(element) || _restoreFocusScope == null || !IsCommandContainerLosingFocus(args.OldFocus, args.NewFocus) || args.NewFocus == null)
            return;
        var restoreFocusScope = _restoreFocusScope;
        _restoreFocusScope = null;
        if (IsAttachedCommandFocusElement(element))
            return;
        restoreFocusScope.PerformRestoration();
        args.Handled = true;
    }

    private static void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs args)
    {
        if (!IsCurrentThreadMainUIThread() || !(sender is DependencyObject element) || !IsRegisteredCommandFocusElement(element))
            return;
        if (IsCommandContainerLosingFocus(args.OldFocus, args.NewFocus))
        {
            CurrentMenuModeSource = null;
            if (args.NewFocus == null)
                return;
            CancelRestoreFocus();
        }
        else
        {
            if (!IsInsideAttachedCommandFocusElement(args.NewFocus))
                return;
            CurrentMenuModeSource = null;
        }
    }

    private static bool IsCommandContainerLosingFocus(IInputElement oldFocus, IInputElement newFocus)
    {
        return IsCommandContainerGainingFocus(newFocus, oldFocus);
    }

    private static bool IsCommandContainerGainingFocus(IInputElement? oldFocus, IInputElement newFocus)
    {
        if (!IsInsideCommandContainer(newFocus))
            return false;
        return oldFocus == null || !IsInsideCommandContainer(oldFocus);
    }

    internal static bool IsAttachedCommandFocusElement(DependencyObject element)
    {
        return CommandNavigationHelper.GetCommandFocusMode(element) == CommandNavigationHelper.CommandFocusMode.Attached;
    }

    private static void PreventFocusScopeCommandRedirection(DependencyObject? newFocus)
    {
        if (newFocus == null)
            return;
        var parentFocusScope = GetParentFocusScope(FocusManager.GetFocusScope(newFocus));
        if (parentFocusScope == null)
            return;
        FocusManager.SetFocusedElement(parentFocusScope, null);
    }

    private static DependencyObject? GetParentFocusScope(DependencyObject? focusScope)
    {
        var visualOrLogicalParent = focusScope?.GetVisualOrLogicalParent();
        return visualOrLogicalParent != null ? FocusManager.GetFocusScope(visualOrLogicalParent) : null;
    }


    private static bool IsCurrentThreadMainUIThread()
    {
        return Application.Current.Dispatcher.CheckAccess();
    }
}