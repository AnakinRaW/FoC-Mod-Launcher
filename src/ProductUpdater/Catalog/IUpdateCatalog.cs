using System.Collections.Generic;
using AnakinRaW.ProductMetadata;

namespace AnakinRaW.AppUpaterFramework.Catalog;

public interface IUpdateCatalog
{
    IProductReference Product { get; }

    IReadOnlyCollection<IUpdateItem> UpdateItems { get; }

    UpdateCatalogAction Action { get; }
}