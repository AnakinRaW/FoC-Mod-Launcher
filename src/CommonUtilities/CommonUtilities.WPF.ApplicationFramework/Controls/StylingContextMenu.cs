using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Utilities;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Controls;

public class StylingContextMenu : ThemedContextMenu, IHasCommandBarStyles
{
    private static ResourceKey? _buttonStyleKey;
    private static ResourceKey? _menuStyleKey;
    private static ResourceKey? _separatorStyleKey;

    public static ResourceKey ButtonStyleKey => _buttonStyleKey ??= new StyleKey<StylingContextMenu>();

    public static ResourceKey MenuStyleKey => _menuStyleKey ??= new StyleKey<StylingContextMenu>();

    public static ResourceKey SeparatorStyleKey => _separatorStyleKey ??= new StyleKey<StylingContextMenu>();

    ResourceKey IHasCommandBarStyles.ButtonStyleKey => ButtonStyleKey;

    ResourceKey IHasCommandBarStyles.MenuStyleKey => MenuStyleKey;

    ResourceKey IHasCommandBarStyles.SeparatorStyleKey => SeparatorStyleKey;

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new DynamicStyledMenuItem();
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        CommandBarStylingUtilities.SelectStyleForItem(element as FrameworkElement, item, this);
    }
}