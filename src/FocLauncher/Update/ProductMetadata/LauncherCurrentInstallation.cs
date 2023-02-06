using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using AnakinRaW.ProductMetadata;
using AnakinRaW.ProductMetadata.Component;
using AnakinRaW.ProductMetadata.Services;
using AnakinRaW.ProductMetadata.Services.Detectors;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace FocLauncher.Update.ProductMetadata;

internal static class CurrentInstalledManifest
{
    // Because we don't store an external manifest we have a few limitations:
    // a) We cannot add a HashCode or FileSize to this manifest, since it's not known at compile time.
    // b) We cannot use AppVars
    // For dependencies of this repository e.g., the AppUpdater we assume the same FileVersion as this assembly.
    public static IReadOnlyCollection<IProductComponent> Components { get; }

    private static readonly ICollection<(string componentName, string location)> ExpectedComponents =
        new List<(string componentName, string location)>
        {

        };

    static CurrentInstalledManifest()
    {
        var components = new List<IProductComponent>();
        var version = LauncherAssemblyInfo.FileVersion;

        foreach (var expectedComponent in ExpectedComponents)
        {
        }

        Components = components;
    }
}

internal class LauncherCurrentInstallation : ICurrentInstallation
{
    private readonly IServiceProvider _serviceProvider;
    private IDirectoryInfo? _installLocation;
    private readonly ILauncherEnvironment _launcherEnvironment;

    public IDirectoryInfo InstallLocation => _installLocation ??= GetInstallLocation();

    public LauncherCurrentInstallation(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
        _launcherEnvironment = serviceProvider.GetRequiredService<ILauncherEnvironment>();
    }

    public IReadOnlyCollection<IProductComponent> GetManifestComponents()
    {
        return CurrentInstalledManifest.Components;
    }

    public IReadOnlyCollection<IProductComponent> FindInstalledComponents()
    {
        var manifest = GetManifestComponents();
        var detectionService = _serviceProvider.GetRequiredService<ICatalogDetectionService>();
        detectionService.UpdateDetectionState(manifest, new VariableCollection());
        return manifest.Where(x => x.DetectedState == DetectionState.Present).ToList();
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