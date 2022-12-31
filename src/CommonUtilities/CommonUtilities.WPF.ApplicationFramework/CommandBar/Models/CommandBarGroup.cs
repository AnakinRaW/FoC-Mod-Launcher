using System.Collections.Generic;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Models;

internal class CommandBarGroup : ICommandBarGroup
{
    public IReadOnlyList<ICommandBarItemDefinition> Items { get; }

    public CommandBarGroup(IReadOnlyList<ICommandBarItemDefinition> items)
    {
        Items = items;
    }
}