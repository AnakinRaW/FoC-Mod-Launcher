using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using AnakinRaW.ProductMetadata;
using AnakinRaW.ProductMetadata.Component;
using AnakinRaW.ProductMetadata.Services;
using AnakinRaW.ProductMetadata.Services.Detectors;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace FocLauncher.Update.ProductMetadata;

internal class LauncherCurrentInstallation : ICurrentInstallation
{
    private readonly IServiceProvider _serviceProvider;
    private IDirectoryInfo? _installLocation;

    public IDirectoryInfo InstallLocation => _installLocation ??= GetInstallLocation();

    public LauncherCurrentInstallation(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider; ILauncherEnvironment
    }

    public IReadOnlyCollection<IProductComponent> GetManifestComponents()
    {
        return Array.Empty<IProductComponent>();
    }

    public IReadOnlyCollection<IProductComponent> FindInstalledComponents()
    {
        var manifest = GetManifestComponents();
        var detectionService = _serviceProvider.GetRequiredService<ICatalogDetectionService>();
        detectionService.UpdateDetectionState(manifest, new VariableCollection());
        return manifest;
    }

    private IDirectoryInfo GetInstallLocation()
    {
        var fs = _serviceProvider.GetRequiredService<IFileSystem>();
        var locationPath = Assembly.GetExecutingAssembly().Location;
        var directory = fs.FileInfo.New(locationPath).Directory;
        if (directory is null || !directory.Exists)
            throw new DirectoryNotFoundException("Unable to find location of current assembly.");
        return directory;
    }
}