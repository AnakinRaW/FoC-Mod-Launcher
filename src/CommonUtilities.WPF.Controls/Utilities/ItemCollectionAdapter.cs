using System.Collections;
using System.Windows.Data;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.Utilities;

internal class ItemCollectionAdapter : CollectionAdapter<object, object>
{
    public ItemCollectionAdapter(IEnumerable source)
    {
        Requires.NotNull(source, nameof(source));
        Initialize(CollectionViewSource.GetDefaultView(source)); ;
    }

    protected override object AdaptItem(object item)
    {
        return item;
    }
}