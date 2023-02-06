﻿using System.Collections.Generic;
using AnakinRaW.ProductMetadata.Component;

namespace AnakinRaW.ProductMetadata.Catalog;

public interface IProductCatalog
{
    IProductReference Product { get; }

    IReadOnlyCollection<IProductComponent> Items { get; }
}