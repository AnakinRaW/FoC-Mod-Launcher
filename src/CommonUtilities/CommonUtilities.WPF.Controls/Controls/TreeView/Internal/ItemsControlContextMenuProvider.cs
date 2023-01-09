using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using AnakinRaW.CommonUtilities.Wpf.Input;

namespace AnakinRaW.CommonUtilities.Wpf.Controls;

internal class ItemsControlContextMenuProvider : ItemsProviderCollector<IHasContextMenu, IContextMenuProvider>,  IContextMenuProvider<IEnumerable<IHasContextMenu>>
{
    private static ItemsControlContextMenuProvider? _instance;
    public static ItemsControlContextMenuProvider Instance => _instance ??= new ItemsControlContextMenuProvider();

    private ItemsControlContextMenuProvider()
    {
    }

    public ContextMenu? Provide(IEnumerable<IHasContextMenu> data)
    {
        var providerGroup = CollectProviders(data, i => i.ContextMenuProvider);
        if (providerGroup.Count != 1)
            return null;
        var firstGroup = providerGroup.First();
        return firstGroup.Key.Provide(firstGroup.Value);
    }

    ContextMenu? IContextMenuProvider.Provide(object data)
    {
        return data is not IEnumerable<IHasContextMenu> items ? null : Provide(items);
    }
}