using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Sklavenwalker.CommonUtilities.Wpf.Utils;
using Sklavenwalker.Wpf.CommandBar.Theming;

namespace Sklavenwalker.Wpf.CommandBar.Controls;

public class ExtendedMenuItem : ThemedMenuItem, IExposeStyleKeys, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public static DependencyProperty IsPlacedOnToolBarProperty = DependencyProperty.Register(nameof(IsPlacedOnToolBar),
        typeof(bool), typeof(ExtendedMenuItem), new FrameworkPropertyMetadata(false));

    public static DependencyProperty IsUserCreatedMenuProperty = DependencyProperty.Register(nameof(IsUserCreatedMenu),
        typeof(bool), typeof(ExtendedMenuItem), new FrameworkPropertyMetadata(false));

    private static ResourceKey? _buttonStyleKey;
    private static ResourceKey? _menuControllerStyleKey;
    private static ResourceKey? _comboBoxStyleKey;
    private static ResourceKey? _menuStyleKey;
    private static ResourceKey? _separatorStyleKey;

    public bool IsPlacedOnToolBar
    {
        get => (bool)GetValue(IsPlacedOnToolBarProperty);
        set => SetValue(IsPlacedOnToolBarProperty, value);
    }

    public bool IsUserCreatedMenu
    {
        get => (bool)GetValue(IsUserCreatedMenuProperty);
        set => SetValue(IsUserCreatedMenuProperty, value);
    }

    public static ResourceKey ButtonStyleKey => _buttonStyleKey ??= new StyleKey<ExtendedMenuItem>();

    public static ResourceKey MenuControllerStyleKey => _menuControllerStyleKey ??= new StyleKey<ExtendedMenuItem>();

    public static ResourceKey ComboBoxStyleKey => _comboBoxStyleKey ??= new StyleKey<ExtendedMenuItem>();

    public static ResourceKey MenuStyleKey => _menuStyleKey ??= new StyleKey<ExtendedMenuItem>();

    public new static ResourceKey SeparatorStyleKey => _separatorStyleKey ??= new StyleKey<ExtendedMenuItem>();


    ResourceKey IExposeStyleKeys.ButtonStyleKey => ButtonStyleKey;

    ResourceKey IExposeStyleKeys.MenuControllerStyleKey => MenuControllerStyleKey;

    ResourceKey IExposeStyleKeys.ComboBoxStyleKey => ComboBoxStyleKey;

    ResourceKey IExposeStyleKeys.MenuStyleKey => MenuStyleKey;

    ResourceKey IExposeStyleKeys.SeparatorStyleKey => SeparatorStyleKey;

    // TODO: Real sub-type
    public Button? HostContainer => this.FindAncestor<Button>();


    protected override void OnVisualParentChanged(DependencyObject oldParent)
    {
        //NotifyPropertyChanged("ParentToolBar");
        NotifyPropertyChanged("HostContainer");
        base.OnVisualParentChanged(oldParent);
    }

    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        if (e.NewFocus != this || GetTemplateChild("PART_FocusTarget") is not UIElement templateChild)
            return;
        templateChild.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        // TODO
    }

    protected override void OnSubmenuOpened(RoutedEventArgs e)
    {
        // TODO: Decide if we want build items here or before we open it.
        //if (!(DataContext is IVsUIDataSource dataContext))
        //    return;
        //if (ServiceProvider.GlobalProvider.GetService(typeof(SVsMenuEventsServiceHelperPrivate)) is IVsMenuEventsServiceHelperPrivate service)
        //    service.NotifyOnBeforeMenuDisplayed(dataContext);
        //dataContext.Invoke("UpdateItems", null, out object _);
        base.OnSubmenuOpened(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        // TODO: Return if ToolbarHosted
        base.OnKeyDown(e);
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new ExtendedMenuItem();
    }

    private void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
