using AnakinRaW.AppUpdaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Metadata.Update;

namespace AnakinRaW.AppUpdaterFramework.Updater;

internal interface IUpdateCatalogProvider
{
    IUpdateCatalog Create(IInstalledProduct installedProduct, IInstalledComponentsCatalog currentCatalog, IProductManifest availableManifest);
}