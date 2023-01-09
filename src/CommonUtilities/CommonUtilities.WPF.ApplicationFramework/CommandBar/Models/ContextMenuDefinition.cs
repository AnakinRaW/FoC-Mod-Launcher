using System.Collections.Generic;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Builder;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Models;

public sealed class ContextMenuDefinition : ICommandBarRootDefinition
{
    public IReadOnlyList<ICommandBarGroup> Groups { get; }

    internal ContextMenuDefinition(IReadOnlyList<ICommandBarGroup> groups)
    {
        Groups = groups;
    }

    public static ICommandBarBuilderStartContext<ContextMenuDefinition> Builder()
    {
        return new ContextMenuModelBuilder();
    }
}