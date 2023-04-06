using System;
using System.IO;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Product;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.ApplicationBase.Update.ApplicationImplementations;

internal class ApplicationProductService : ProductServiceBase
{
    private readonly IBranchManager _branchManager;
    private readonly IApplicationEnvironment _applicationEnvironment;

    private IDirectoryInfo? _installLocation;

    public override IDirectoryInfo InstallLocation => _installLocation ??= GetInstallLocation();


    public ApplicationProductService(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _branchManager = serviceProvider.GetRequiredService<IBranchManager>();
        _applicationEnvironment = serviceProvider.GetRequiredService<IApplicationEnvironment>();
    }

    protected override IProductReference CreateCurrentProductReference()
    {
        var version = _applicationEnvironment.AssemblyInfo.InformationalVersion;
        var branch = _branchManager.GetBranchFromVersion(version);
        return new ProductReference(_applicationEnvironment.ApplicationName, version, branch);
    }

    protected override void AddAdditionalProductVariables(ProductVariables variables, IProductReference product)
    {
        variables.Add(ApplicationVariablesKeys.AppData, _applicationEnvironment.ApplicationLocalPath);
        variables.Add(ApplicationVariablesKeys.ApplicationFileName, _applicationEnvironment.AssemblyInfo.ExecutableFileName);
    }


    private IDirectoryInfo GetInstallLocation()
    {
        var fs = ServiceProvider.GetRequiredService<IFileSystem>();
        var locationPath = _applicationEnvironment.AssemblyInfo.CurrentAssembly.Location;
        var directory = fs.FileInfo.New(locationPath).Directory;
        if (directory is null || !directory.Exists)
            throw new DirectoryNotFoundException("Unable to find location of current assembly.");
        return directory;
    }
}