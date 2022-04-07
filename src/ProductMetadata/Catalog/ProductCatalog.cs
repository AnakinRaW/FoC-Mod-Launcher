using System.Collections.Generic;
using Sklavenwalker.ProductMetadata.Component;
using Validation;

namespace Sklavenwalker.ProductMetadata.Catalog;

public class ProductCatalog : IProductCatalog
{
    public IProductReference Product { get; }

    public IEnumerable<IProductComponent> Items { get; }

    public ProductCatalog(IProductReference product, IEnumerable<IProductComponent> components)
    {
        Requires.NotNull(product, nameof(product));
        Requires.NotNull(components, nameof(components));
        Items = components;
        Product = product;
    }
}