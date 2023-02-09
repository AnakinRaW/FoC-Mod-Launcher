using AnakinRaW.AppUpaterFramework.Catalog;
using AnakinRaW.ProductMetadata;
using AnakinRaW.ProductMetadata.Catalog;

namespace AnakinRaW.AppUpaterFramework.Services;

internal interface IUpdateCatalogProvider
{
    IUpdateCatalog Create(IInstalledComponentsCatalog currentCatalog, IProductManifest availableManifest, VariableCollection productVariables);
}