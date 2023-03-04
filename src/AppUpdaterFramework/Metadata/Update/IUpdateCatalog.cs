using System.Collections.Generic;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;

namespace AnakinRaW.AppUpdaterFramework.Metadata.Update;

public interface IUpdateCatalog
{
    IInstalledProduct InstalledProduct { get; }

    IProductReference UpdateReference { get; }

    IReadOnlyCollection<IUpdateItem> UpdateItems { get; }

    UpdateCatalogAction Action { get; }
}