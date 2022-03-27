using Sklavenwalker.ProductMetadata;

namespace Sklavenwalker.ProductUpdater
{
    public interface IUpdateCatalogBuilder
    {
        IUpdateCatalog Build(IInstalledProductCatalog installedCatalog, IAvailableProductManifest availableCatalog);
    }
}