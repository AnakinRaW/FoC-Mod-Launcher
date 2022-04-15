using Sklavenwalker.ProductMetadata.Catalog;
using Sklavenwalker.ProductUpdater.Catalog;

namespace Sklavenwalker.ProductUpdater;

public interface IUpdateCatalogBuilder
{
    IUpdateCatalog Build(IInstalledProductCatalog installedCatalog, IProductCatalog availableCatalog);
}