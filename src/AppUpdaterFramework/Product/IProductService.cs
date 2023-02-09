using System.IO.Abstractions;
using AnakinRaW.AppUpaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using Semver;

namespace AnakinRaW.AppUpaterFramework.Product;

public interface IProductService
{
    IDirectoryInfo InstallLocation { get; }

    IInstalledProduct GetCurrentInstance();

    IProductReference CreateProductReference(SemVersion? newVersion, ProductBranch? branch);

    IInstalledComponentsCatalog GetInstalledComponents();

    bool IsProductCompatible(IProductReference product);

    void RevalidateInstallation();
}