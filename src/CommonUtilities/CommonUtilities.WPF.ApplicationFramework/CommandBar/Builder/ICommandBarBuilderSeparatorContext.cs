using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Models;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Builder;

public interface ICommandBarBuilderSeparatorContext<T> where T : class, ICommandBarItemsSource
{
    ICommandBarBuilderContext<T> AddItem(ICommandBarItemDefinition item);
}