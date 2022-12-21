﻿using System.Collections.Generic;
using Sklavenwalker.ProductMetadata.Component;

namespace Sklavenwalker.ProductMetadata.Catalog;

public interface IProductCatalog
{
    IProductReference Product { get; }

    IReadOnlyList<IProductComponent> Items { get; }
}