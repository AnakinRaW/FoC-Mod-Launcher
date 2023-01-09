using System.Collections.Generic;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Models;

public interface ICommandBarGroup
{
    IReadOnlyList<ICommandBarItemDefinition> Items { get; }
}