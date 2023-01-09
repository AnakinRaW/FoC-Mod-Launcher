﻿using System.Collections.Generic;
using AnakinRaW.ProductMetadata.Component;
using Validation;

namespace AnakinRaW.ProductMetadata.Catalog;

public class InstalledProductCatalog : ProductCatalog, IInstalledProductCatalog
{
    public new IInstalledProduct Product { get; }

    public InstalledProductCatalog(IInstalledProduct product, IReadOnlyList<IProductComponent> components) : base(product, components)
    {
        Requires.NotNull(product, nameof(product));
        Product = product;
    }
}