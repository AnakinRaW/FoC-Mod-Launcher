using AnakinRaW.AppUpaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.AppUpaterFramework.Metadata.Update;

namespace AnakinRaW.AppUpaterFramework.Updater;

internal interface IUpdateCatalogProvider
{
    IUpdateCatalog Create(IInstalledProduct installedProduct, IInstalledComponentsCatalog currentCatalog, IProductManifest availableManifest);
}