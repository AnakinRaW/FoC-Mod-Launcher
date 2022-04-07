using Sklavenwalker.ProductMetadata.Catalog;

namespace Sklavenwalker.ProductUpdater
{
    public interface IUpdateCatalogBuilder
    {
        IUpdateCatalog Build(IInstalledProductCatalog installedCatalog, IAvailableProductManifest availableCatalog);
    }
}