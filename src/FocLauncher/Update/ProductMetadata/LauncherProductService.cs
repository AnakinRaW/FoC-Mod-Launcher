using System;
using AnakinRaW.ProductMetadata;
using AnakinRaW.ProductMetadata.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FocLauncher.Update.ProductMetadata;

internal class LauncherProductService : ProductServiceBase
{
    private readonly IBranchManager _branchManager;
    
    public LauncherProductService(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _branchManager = serviceProvider.GetRequiredService<IBranchManager>();
    }

    protected override IInstalledProduct BuildProduct()
    {
        var productRef = CreateCurrentProductReference();
        var installLocation = CurrentInstallation.InstallLocation.FullName;
        return new InstalledProduct(productRef, installLocation, ProductInstallState.RestartPending);
    }

    private IProductReference CreateCurrentProductReference()
    {
        const string name = LauncherConstants.ApplicationName;
        var version = LauncherAssemblyInfo.InformationalAsSemVer();
        ProductBranch? branch = null;
        if (version is not null)
            branch = _branchManager.GetBranchFromVersion(version);
        return new ProductReference(name, version, branch);
    }
}