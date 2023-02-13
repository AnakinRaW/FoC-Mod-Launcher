using System.Collections.Generic;
using AnakinRaW.AppUpaterFramework.Metadata.Product;

namespace AnakinRaW.AppUpaterFramework.Metadata.Update;

public interface IUpdateCatalog
{
    IInstalledProduct InstalledProduct { get; }

    IProductReference UpdateReference { get; }

    IReadOnlyCollection<IUpdateItem> UpdateItems { get; }

    UpdateCatalogAction Action { get; }
}