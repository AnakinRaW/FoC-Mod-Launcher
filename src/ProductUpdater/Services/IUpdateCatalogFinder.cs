using Sklavenwalker.ProductMetadata;
using Sklavenwalker.ProductMetadata.Catalog;

namespace Sklavenwalker.ProductUpdater.Services;

public interface IUpdateCatalogFinder
{
    CatalogLocation Find(IInstalledProduct product, string branch);
}