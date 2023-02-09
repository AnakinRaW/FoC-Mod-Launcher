using System.Collections.Generic;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using Validation;

namespace AnakinRaW.AppUpaterFramework.Metadata.Component.Catalog;

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