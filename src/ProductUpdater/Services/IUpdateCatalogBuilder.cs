using AnakinRaW.ProductMetadata.Catalog;
using AnakinRaW.ProductUpdater.Catalog;

namespace AnakinRaW.ProductUpdater.Services;

internal interface IUpdateCatalogBuilder
{
    IUpdateCatalog Build(IInstalledProductCatalog installedCatalog, IProductCatalog availableCatalog);
}