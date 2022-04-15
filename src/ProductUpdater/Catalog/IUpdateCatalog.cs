using System.Collections.Generic;
using Sklavenwalker.ProductMetadata;

namespace Sklavenwalker.ProductUpdater.Catalog;

public interface IUpdateCatalog
{
    IProductReference Product { get; }

    IReadOnlyCollection<IUpdateItem> UpdateItems { get; }
}