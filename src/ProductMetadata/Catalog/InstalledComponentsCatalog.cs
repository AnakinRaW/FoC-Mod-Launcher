using System.Collections.Generic;
using AnakinRaW.ProductMetadata.Component;
using Validation;

namespace AnakinRaW.ProductMetadata.Catalog;

public class InstalledComponentsCatalog : IInstalledComponentsCatalog
{
    public IProductReference Product { get; }

    public IReadOnlyCollection<IInstallableComponent> Items { get; }

    public InstalledComponentsCatalog(IProductReference product, IReadOnlyCollection<IInstallableComponent> items)
    {
        Requires.NotNull(product, nameof(product));
        Product = product;
        Items = items;
    }
}