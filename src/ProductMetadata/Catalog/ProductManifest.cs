using System.Collections.Generic;
using AnakinRaW.ProductMetadata.Component;
using Validation;

namespace AnakinRaW.ProductMetadata.Catalog;

public class ProductManifest : IProductManifest
{
    public IProductReference Product { get; }

    public IReadOnlyCollection<IProductComponent> Items { get; }

    public ProductManifest(IProductReference product, IReadOnlyCollection<IProductComponent> components)
    {
        Requires.NotNull(product, nameof(product));
        Requires.NotNull(components, nameof(components));
        Items = components;
        Product = product;
    }
}