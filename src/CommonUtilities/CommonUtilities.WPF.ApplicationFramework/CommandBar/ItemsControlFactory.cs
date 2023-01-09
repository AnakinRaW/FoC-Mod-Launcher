using System;
using System.Collections.Generic;
using System.Linq;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Models;
using Validation;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.CommandBar;

internal class ItemsControlFactory
{
    public IReadOnlyList<ICommandBarControlViewModel> CreateModel(ICommandBarItemsSource itemsSource)
    {
        Requires.NotNull(itemsSource, nameof(itemsSource));
        var groups = itemsSource.Groups;
        if (!groups.Any())
            throw new InvalidOperationException("Unable to create empty menu");

        var controlItems = new List<ICommandBarControlViewModel>();

        HandleGroup(groups.First(), controlItems);

        foreach (var group in groups.Skip(1))
        {
            controlItems.Add(new SeparatorControlViewModel());
            HandleGroup(group, controlItems);
        }

        return controlItems;
    }

    private void HandleGroup(ICommandBarGroup group, ICollection<ICommandBarControlViewModel> items)
    {
        foreach (var itemDefinition in group.Items)
        {
            switch (itemDefinition)
            {
                case IMenuDefinition menuItem:
                    var menuControl =
                        new MenuItemControlViewModel(menuItem.Text, CreateModel(menuItem))
                        {
                            Tooltip = menuItem.ToolTop,
                            IsEnabled = menuItem.IsEnabled
                        };
                    items.Add(menuControl);
                    break;
                case ICommandItemDefinition commandItem:
                    items.Add(new ButtonControlViewModel(commandItem.CommandDefinition));
                    break;
            }
        }
    }
}