using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using AnakinRaW.AppUpdaterFramework.Conditions;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Product.Manifest;
using AnakinRaW.ExternalUpdater.CLI;
using FocLauncher.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace FocLauncher.Update.LauncherImplementations;

internal class LauncherInstalledManifestProvider : IInstalledManifestProvider
{
    private readonly IFileSystem _fileSystem;
    private readonly IExternalUpdaterService _externalUpdaterService;
    private readonly ICosturaResourceExtractor _resourceExtractor;

    public LauncherInstalledManifestProvider(IServiceProvider serviceProvider)
    {
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _externalUpdaterService = serviceProvider.GetRequiredService<IExternalUpdaterService>();
        _resourceExtractor = serviceProvider.GetRequiredService<ICosturaResourceExtractor>();
    }

    public IProductManifest ProvideManifest(IProductReference installedProduct, ProductVariables variables)
    {
        var components = BuildManifestComponents(installedProduct).ToList();
        return new ProductManifest(installedProduct, components);
    }

    // Because we don't store an external manifest we have a few limitations:
    // a) We cannot add a HashCode or FileSize to this manifest, since it's not known at compile time.
    private IEnumerable<IProductComponent> BuildManifestComponents(IProductReference installedProduct)
    {
        var identityVersion = installedProduct.Version;
        var fileVersion = LauncherAssemblyInfo.FileVersion;

        var launcherExeId = new ProductComponentIdentity("Launcher.Executable", identityVersion);
        
        var launcherExecutable = BuildFileComponent(launcherExeId, "FoC Mod Launcher", $"[{KnownProductVariablesKeys.InstallDir}]",
            LauncherAssemblyInfo.ExecutableFileName, fileVersion);

        var assemblyStream = GetUpdaterAssemblyStream();
        var updater = _externalUpdaterService.GetExternalUpdaterComponent(assemblyStream, $"[{LauncherVariablesKeys.LauncherAppData}]");

        yield return new ComponentGroup(new ProductComponentIdentity(LauncherConstants.ApplicationCoreGroupId, identityVersion), new List<IProductComponentIdentity>
        {
            launcherExeId,
            updater,
        })
        {
            Name = $"{LauncherAssemblyInfo.ProductName}"
        };
        yield return launcherExecutable;
        yield return updater;
    }

    private Stream GetUpdaterAssemblyStream()
    {
        var task = Task.Run(async () => await _resourceExtractor.GetResourceAsync(ExternalUpdaterConstants.AppUpdaterModuleName));
        task.Wait();
        return task.Result;
    }


    private SingleFileComponent BuildFileComponent(IProductComponentIdentity identity, string name, string directoryKey, string fileName, string version)
    {
        var filePath = _fileSystem.Path.Combine(directoryKey, fileName);
        return new SingleFileComponent(identity, directoryKey, fileName, null)
        {
            Name = name,
            DetectConditions = new[]
            {
                new FileCondition(filePath)
                {
                    Version =  Version.Parse(version)
                }
            }
        };
    }
}