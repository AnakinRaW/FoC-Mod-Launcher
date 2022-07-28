using Semver;
using Sklavenwalker.ProductMetadata.Catalog;

namespace Sklavenwalker.ProductMetadata.Services;

public interface IProductService
{
    IInstalledProduct GetCurrentInstance();

    IProductReference CreateProductReference(SemVersion? newVersion, ProductBranch? branch);

    IInstalledProductCatalog GetInstalledProductCatalog();

    bool IsProductCompatible(IProductReference product);
}