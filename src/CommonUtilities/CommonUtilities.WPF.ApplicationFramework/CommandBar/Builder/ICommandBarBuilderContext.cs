using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Models;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Builder;

public interface ICommandBarBuilderContext<T> where T : class, ICommandBarItemsSource
{
    ICommandBarBuilderContext<T> AddItem(ICommandBarItemDefinition item);

    ICommandBarBuilderSeparatorContext<T> AddSeparator();

    T Build();
}