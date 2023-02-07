using System.Collections.Generic;
using AnakinRaW.ProductMetadata.Component;
using Validation;

namespace AnakinRaW.ProductMetadata.Catalog;

public class InstalledProductCatalog : IInstalledProductCatalog
{
    public IInstalledProduct Product { get; }

    public IReadOnlyCollection<IInstallableComponent> Items { get; }

    IProductReference IProductCatalog<IInstallableComponent>.Product => Product;

    public InstalledProductCatalog(IInstalledProduct product, IReadOnlyCollection<IInstallableComponent> items)
    {
        Requires.NotNull(product, nameof(product));
        Product = product;
        Items = items;
    }
}