using System.Collections.Generic;
using System.Linq;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using Validation;

namespace AnakinRaW.AppUpaterFramework.Metadata.Update;

public class UpdateCatalog : IUpdateCatalog
{
    public IProductReference Product { get; }

    public IReadOnlyCollection<IUpdateItem> UpdateItems { get; }

    public UpdateCatalogAction Action { get; }

    public UpdateCatalog(IProductReference product, IEnumerable<IUpdateItem> updateItems, UpdateCatalogAction action = UpdateCatalogAction.Update)
    {
        Requires.NotNull(product, nameof(product));
        Product = product;
        UpdateItems = updateItems.ToList();
        Action = action;
    }

    internal static UpdateCatalog CreateEmpty(IProductReference product)
    {
        return new UpdateCatalog(product, Enumerable.Empty<IUpdateItem>(), UpdateCatalogAction.None);
    }
}