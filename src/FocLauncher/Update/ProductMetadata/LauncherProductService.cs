using System;
using System.IO.Abstractions;
using System.IO;
using System.Reflection;
using AnakinRaW.ProductMetadata;
using AnakinRaW.ProductMetadata.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FocLauncher.Update.ProductMetadata;

internal class LauncherProductService : ProductServiceBase
{
    private readonly IBranchManager _branchManager;

    private IDirectoryInfo? _installLocation;

    public override IDirectoryInfo InstallLocation => _installLocation ??= GetInstallLocation();


    public LauncherProductService(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _branchManager = serviceProvider.GetRequiredService<IBranchManager>();
    }

    protected override IProductReference CreateCurrentProductReference()
    {
        const string name = LauncherConstants.ApplicationName;
        var version = LauncherAssemblyInfo.InformationalAsSemVer();
        ProductBranch? branch = null;
        if (version is not null)
            branch = _branchManager.GetBranchFromVersion(version);
        return new ProductReference(name, version, branch);
    }

    protected override void AddAdditionalProductVariables(VariableCollection variables, IProductReference product)
    {
        base.AddAdditionalProductVariables(variables, product);
    }

    protected override ProductInstallState FetchInstallState(IProductReference productReference)
    {
        return ProductInstallState.RestartPending;
    }

    private IDirectoryInfo GetInstallLocation()
    {
        var fs = ServiceProvider.GetRequiredService<IFileSystem>();
        var locationPath = Assembly.GetExecutingAssembly().Location;
        var directory = fs.FileInfo.New(locationPath).Directory;
        if (directory is null || !directory.Exists)
            throw new DirectoryNotFoundException("Unable to find location of current assembly.");
        return directory;
    }
}