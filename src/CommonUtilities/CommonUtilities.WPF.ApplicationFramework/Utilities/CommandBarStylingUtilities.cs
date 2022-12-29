using System;
using System.Windows;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Utilities;

internal class CommandBarStylingUtilities
{
    internal static void SelectStyleForItem(FrameworkElement element, object item, IHasCommandBarStyles styleKeySource)
    {
        if (item is not ICommandBarControlViewModel viewModel)
            return;
        ResourceKey resourceKey;
        switch (viewModel.Type)
        {
            case CommandBarType.Button:
                resourceKey = styleKeySource.ButtonStyleKey;
                break;
            case CommandBarType.Menu:
                resourceKey = styleKeySource.MenuStyleKey;
                break;
            case CommandBarType.Separator:
                resourceKey = styleKeySource.SeparatorStyleKey;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        element.SetResourceReference(FrameworkElement.StyleProperty, resourceKey);
    }
}