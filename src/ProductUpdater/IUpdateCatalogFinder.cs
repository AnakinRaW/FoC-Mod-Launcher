using Sklavenwalker.ProductMetadata;
using Sklavenwalker.ProductMetadata.Catalog;

namespace Sklavenwalker.ProductUpdater;

public interface IUpdateCatalogFinder
{
    CatalogLocation Find(IInstalledProduct product);
}