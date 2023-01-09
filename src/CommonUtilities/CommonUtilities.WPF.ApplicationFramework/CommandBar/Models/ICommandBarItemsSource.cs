using System.Collections.Generic;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Models;

public interface ICommandBarItemsSource
{
    IReadOnlyList<ICommandBarGroup> Groups { get; }
}