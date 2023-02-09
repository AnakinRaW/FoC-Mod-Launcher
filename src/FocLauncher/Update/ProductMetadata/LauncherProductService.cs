﻿using System;
using System.IO.Abstractions;
using System.IO;
using System.Reflection;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.AppUpaterFramework.Product;
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

    protected override void AddAdditionalProductVariables(ProductVariables variables, IProductReference product)
    {
        var env = ServiceProvider.GetRequiredService<ILauncherEnvironment>();
        variables.Add(LauncherVariablesKeys.LauncherAppData, env.ApplicationLocalPath);
        variables.Add(LauncherVariablesKeys.LauncherFileName, LauncherAssemblyInfo.AssemblyName);
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

internal static class LauncherVariablesKeys
{
    public const string LauncherAppData = "LauncherAppData";
    public const string LauncherFileName = "LauncherFileName";
}