﻿using System.Windows;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Utilities;
using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Controls;

public class DynamicStyledMenuItem : ThemedMenuItem, IHasCommandBarStyles
{
    private static ResourceKey? _buttonStyleKey;
    private static ResourceKey? _menuStyleKey;
    private static ResourceKey? _separatorStyleKey;

    public static ResourceKey ButtonStyleKey => _buttonStyleKey ??= new StyleKey<DynamicStyledMenuItem>();

    public static ResourceKey MenuStyleKey => _menuStyleKey ??= new StyleKey<DynamicStyledMenuItem>();

    public static ResourceKey SeparatorStyleKey => _separatorStyleKey ??= new StyleKey<DynamicStyledMenuItem>();

    ResourceKey IHasCommandBarStyles.ButtonStyleKey => ButtonStyleKey;

    ResourceKey IHasCommandBarStyles.MenuStyleKey => MenuStyleKey;

    ResourceKey IHasCommandBarStyles.SeparatorStyleKey => SeparatorStyleKey;

    static DynamicStyledMenuItem()
    {
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new DynamicStyledMenuItem();
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        var frameworkElement = element as FrameworkElement;
        CommandBarStylingUtilities.SelectStyleForItem(frameworkElement, item, this);
    }
}