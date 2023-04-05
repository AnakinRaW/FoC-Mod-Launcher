using System;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Product;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.ApplicationBase.Update.LauncherImplementations;

internal class ApplicationProductService : ProductServiceBase
{
    private readonly IBranchManager _branchManager;

    private IDirectoryInfo? _installLocation;

    public override IDirectoryInfo InstallLocation => _installLocation ??= GetInstallLocation();


    public ApplicationProductService(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _branchManager = serviceProvider.GetRequiredService<IBranchManager>();
    }

    protected override IProductReference CreateCurrentProductReference()
    {
        var env = ServiceProvider.GetRequiredService<IApplicationEnvironment>();
        var version = env.InformationalVersion;
        var branch = _branchManager.GetBranchFromVersion(version);
        return new ProductReference(env.ApplicationName, version, branch);
    }

    protected override void AddAdditionalProductVariables(ProductVariables variables, IProductReference product)
    {
        var env = ServiceProvider.GetRequiredService<IApplicationEnvironment>();
        variables.Add(ApplicationVariablesKeys.AppData, env.ApplicationLocalPath);
        variables.Add(ApplicationVariablesKeys.ApplicationFileName, LauncherAssemblyInfo.ExecutableFileName);
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