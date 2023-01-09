using System.Collections.Generic;
using System.Linq;
using AnakinRaW.ProductMetadata;
using Validation;

namespace AnakinRaW.ProductUpdater.Catalog;

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