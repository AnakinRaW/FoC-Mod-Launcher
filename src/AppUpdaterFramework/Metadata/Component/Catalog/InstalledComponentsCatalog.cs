using System.Collections.Generic;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using Validation;

namespace AnakinRaW.AppUpaterFramework.Metadata.Component.Catalog;

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