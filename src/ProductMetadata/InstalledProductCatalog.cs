﻿using System.Collections.Generic;
using Sklavenwalker.ProductMetadata.Component;
using Validation;

namespace Sklavenwalker.ProductMetadata;

public class InstalledProductCatalog : Catalog, IInstalledProductCatalog
{
    public IInstalledProduct Product { get; }

    public InstalledProductCatalog(IInstalledProduct product, IEnumerable<IProductComponent> components) : base(components)
    {
        Requires.NotNull(product, nameof(product));
        Product = product;
    }
}