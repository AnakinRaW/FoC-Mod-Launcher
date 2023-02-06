using AnakinRaW.ProductMetadata.Catalog;
using Semver;

namespace AnakinRaW.ProductMetadata.Services;

public interface IProductService
{
    IInstalledProduct GetCurrentInstance();

    IProductReference CreateProductReference(SemVersion? newVersion, ProductBranch? branch);

    IInstalledProductCatalog GetInstalledProductCatalog();

    bool IsProductCompatible(IProductReference product);

    void RevalidateInstallation();
}