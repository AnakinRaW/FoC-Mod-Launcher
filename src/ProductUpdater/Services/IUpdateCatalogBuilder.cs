using Sklavenwalker.ProductMetadata.Catalog;
using Sklavenwalker.ProductUpdater.Catalog;

namespace Sklavenwalker.ProductUpdater.Services;

internal interface IUpdateCatalogBuilder
{
    IUpdateCatalog Build(IInstalledProductCatalog installedCatalog, IProductCatalog availableCatalog);
}