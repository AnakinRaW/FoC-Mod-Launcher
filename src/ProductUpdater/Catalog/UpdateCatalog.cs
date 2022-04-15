using System.Collections.Generic;
using System.Linq;
using Sklavenwalker.ProductMetadata;
using Sklavenwalker.ProductMetadata.Catalog;
using Sklavenwalker.ProductMetadata.Component;
using Validation;

namespace Sklavenwalker.ProductUpdater.Catalog;

public class UpdateCatalog : IUpdateCatalog
{
    public IProductReference Product { get; }

    public IReadOnlyCollection<IUpdateItem> UpdateItems { get; }

    public UpdateCatalog(IProductReference product, IEnumerable<IUpdateItem> updateItems)
    {
        Requires.NotNull(product, nameof(product));
        Product = product;
        UpdateItems = updateItems.ToList();
    }

    public static UpdateCatalog CreateEmpty(IProductReference product)
    {
        return new UpdateCatalog(product, Enumerable.Empty<IUpdateItem>());
    }
}