using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using Semver;

namespace AnakinRaW.AppUpdaterFramework.Product;

public interface IProductService
{
    IDirectoryInfo InstallLocation { get; }

    IInstalledProduct GetCurrentInstance();

    IProductReference CreateProductReference(SemVersion? newVersion, ProductBranch? branch);

    IInstalledComponentsCatalog GetInstalledComponents();

    bool IsProductCompatible(IProductReference product);

    void RevalidateInstallation();
}