using System.Windows;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Utilities;
using AnakinRaW.CommonUtilities.Wpf.Controls;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Controls;

public class DynamicStyledMenuItem : ThemedMenuItem, IHasCommandBarStyles
{
    private static ResourceKey? _buttonStyleKey;
    private static ResourceKey? _menuStyleKey;
    private static ResourceKey? _separatorStyleKey;

    public static ResourceKey ButtonStyleKey => _buttonStyleKey ??= new StyleKey<DynamicStyledMenuItem>();

    public static ResourceKey MenuStyleKey => _menuStyleKey ??= new StyleKey<DynamicStyledMenuItem>();

    public new static ResourceKey SeparatorStyleKey => _separatorStyleKey ??= new StyleKey<DynamicStyledMenuItem>();

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
        if (element is not FrameworkElement frameworkElement)
            return;
        CommandBarStylingUtilities.SelectStyleForItem(frameworkElement, item, this);
    }
}