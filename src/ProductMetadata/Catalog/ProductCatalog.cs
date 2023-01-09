using System.Collections.Generic;
using AnakinRaW.ProductMetadata.Component;
using Validation;

namespace AnakinRaW.ProductMetadata.Catalog;

public class ProductCatalog : IProductCatalog
{
    public IProductReference Product { get; }

    public IReadOnlyList<IProductComponent> Items { get; }

    public ProductCatalog(IProductReference product, IReadOnlyList<IProductComponent> components)
    {
        Requires.NotNull(product, nameof(product));
        Requires.NotNull(components, nameof(components));
        Items = components;
        Product = product;
    }
}