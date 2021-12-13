using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Sklavenwalker.CommonUtilities.Wpf.Utils;
using Validation;

namespace Sklavenwalker.Wpf.CommandBar.Input.Focus;

public static class CommandNavigationHelper
{
    public static event PropertyChangedCallback? CommandFocusModePropertyChanged;

    public static readonly DependencyProperty CommandFocusModeProperty =
        DependencyProperty.RegisterAttached("CommandFocusMode", typeof(CommandFocusMode),
            typeof(CommandNavigationHelper), new FrameworkPropertyMetadata(CommandFocusMode.None, OnCommandFocusModePropertyChanged));

    public static readonly DependencyProperty IsCommandNavigableProperty =
        DependencyProperty.RegisterAttached("IsCommandNavigable", typeof(bool), typeof(CommandNavigationHelper),
            new FrameworkPropertyMetadata(false, OnIsCommandNavigablePropertyChanged));

    public static readonly DependencyProperty CommandNavigationOrderProperty =
        DependencyProperty.RegisterAttached("CommandNavigationOrder", typeof(int), typeof(CommandNavigationHelper),
            new FrameworkPropertyMetadata(0, OnCommandNavigationOrderPropertyChanged));

    private static readonly WeakCollection<UIElement> NavigableControls = new();

    private static bool IsCommandNavigationOrderDirty { get; set; }

    public static CommandFocusMode GetCommandFocusMode(DependencyObject element)
    {
        Requires.NotNull(element, nameof(element));
        return (CommandFocusMode)element.GetValue(CommandFocusModeProperty);
    }

    public static void SetCommandFocusMode(DependencyObject element, CommandFocusMode value)
    {
        Requires.NotNull(element, nameof(element));
        element.SetValue(CommandFocusModeProperty, value);
    }
    
    private static void OnCommandFocusModePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        CommandFocusModePropertyChanged?.Invoke(obj, e);
    }

    public static bool GetIsCommandNavigable(DependencyObject element)
    {
        Requires.NotNull(element, nameof(element));
        return (bool)element.GetValue(IsCommandNavigableProperty);
    }

    public static void SetIsCommandNavigable(DependencyObject element, bool value)
    {
        Requires.NotNull(element, nameof(element));
        element.SetValue(IsCommandNavigableProperty, value);
    }

    private static void OnIsCommandNavigablePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        Requires.NotNull(obj, nameof(obj));
        if (obj is not UIElement control)
            return;
        if (e.NewValue is true)
            RegisterNavigableControl(control);
        else
            UnregisterNavigableControl(control);
    }

    public static int GetCommandNavigationOrder(DependencyObject element)
    {
        Requires.NotNull(element, nameof(element));
        return (int)element.GetValue(CommandNavigationOrderProperty);
    }

    public static void SetCommandNavigationOrder(DependencyObject element, int value)
    {
        Requires.NotNull(element, nameof(element));
        element.SetValue(CommandNavigationOrderProperty, value);
        IsCommandNavigationOrderDirty = true;
    }

    private static void OnCommandNavigationOrderPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        IsCommandNavigationOrderDirty = true;
    }
    
    private static void RegisterNavigableControl(UIElement control)
    {
        NavigableControls.Add(control);
        IsCommandNavigationOrderDirty = true;
    }

    private static void UnregisterNavigableControl(UIElement control)
    {
        NavigableControls.Remove(control);
        IsCommandNavigationOrderDirty = true;
    }

    internal static IEnumerable<UIElement> GetSortedNavigableControls()
    {
        if (IsCommandNavigationOrderDirty)
        {
            var list = NavigableControls.OrderBy(GetCommandNavigationOrder).ToList();
            NavigableControls.Clear();
            foreach (var uiElement in list)
                NavigableControls.Add(uiElement);
            IsCommandNavigationOrderDirty = false;
        }
        return NavigableControls;
    }

    public enum CommandFocusMode
    {
        None,
        Attached,
        Container,
    }
}