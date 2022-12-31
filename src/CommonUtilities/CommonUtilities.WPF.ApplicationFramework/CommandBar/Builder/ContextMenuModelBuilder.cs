using System.Collections.Generic;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Models;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Builder;

internal class ContextMenuModelBuilder : CommandBarModelBuilder<ContextMenuDefinition>
{
    protected override ContextMenuDefinition BuildCore(IReadOnlyList<ICommandBarGroup> groups)
    {
        return new ContextMenuDefinition(groups);
    }
}