using System.Collections.Generic;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Models;

public interface ICommandBarGroup
{
    IReadOnlyList<ICommandBarItemDefinition> Items { get; }
}