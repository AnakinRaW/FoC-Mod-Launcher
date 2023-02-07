using AnakinRaW.ProductMetadata.Catalog;
using Semver;
using System.IO.Abstractions;

namespace AnakinRaW.ProductMetadata.Services;

public interface IProductService
{
    IDirectoryInfo InstallLocation { get; }

    IInstalledProduct GetCurrentInstance();

    IProductReference CreateProductReference(SemVersion? newVersion, ProductBranch? branch);

    IInstalledProductCatalog GetInstalledProductCatalog();

    bool IsProductCompatible(IProductReference product);

    void RevalidateInstallation();
}