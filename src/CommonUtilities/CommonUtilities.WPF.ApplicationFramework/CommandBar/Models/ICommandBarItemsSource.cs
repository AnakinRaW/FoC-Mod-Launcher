using System.Collections.Generic;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Models;

public interface ICommandBarItemsSource
{
    IReadOnlyList<ICommandBarGroup> Groups { get; }
}