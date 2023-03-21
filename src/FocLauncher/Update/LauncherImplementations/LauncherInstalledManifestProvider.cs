using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using AnakinRaW.AppUpdaterFramework.Conditions;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Product.Manifest;
using Microsoft.Extensions.DependencyInjection;

namespace FocLauncher.Update.LauncherImplementations;

internal class LauncherInstalledManifestProvider : IInstalledManifestProvider
{
    private readonly IFileSystem _fileSystem;

    public LauncherInstalledManifestProvider(IServiceProvider serviceProvider)
    {
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    }

    public IProductManifest ProvideManifest(IProductReference installedProduct, ProductVariables variables)
    {
        var components = BuildManifestComponents(installedProduct, variables).ToList();
        return new ProductManifest(installedProduct, components);
    }

    // Because we don't store an external manifest we have a few limitations:
    // a) We cannot add a HashCode or FileSize to this manifest, since it's not known at compile time.
    // b) For dependencies of this repository e.g., the AppUpdater we need to assume the same FileVersion as this assembly.
    private IEnumerable<IProductComponent> BuildManifestComponents(IProductReference installedProduct, ProductVariables variables)
    {
        var identityVersion = installedProduct.Version;
        var fileVersion = LauncherAssemblyInfo.FileVersion;

        var launcherExeId = new ProductComponentIdentity("Launcher.Executable", identityVersion);
        var launcherUpdaterId = new ProductComponentIdentity("Launcher.Updater", identityVersion);

        
        var launcherExecutable = BuildFileComponent(launcherExeId, "FoC Mod Launcher", KnownProductVariablesKeys.InstallDir,
            LauncherAssemblyInfo.ExecutableFileName, fileVersion, variables);
        var launcherUpdater = BuildFileComponent(launcherUpdaterId, "Launcher Updater", LauncherVariablesKeys.LauncherAppData,
            LauncherConstants.AppUpdaterAssemblyName, fileVersion, variables);


        //var testExeId = new ProductComponentIdentity("Launcher.Test", identityVersion);
        //var testExe = BuildFileComponent(testExeId, "Launcher Test", LauncherVariablesKeys.LauncherAppData,
        //    LauncherAssemblyInfo.ExecutableFileName, fileVersion, variables);


        yield return new ComponentGroup(new ProductComponentIdentity(LauncherConstants.ApplicationCoreGroupId, identityVersion), new List<IProductComponentIdentity>
        {
            launcherExeId,
            launcherUpdaterId,
           // testExeId
        })
        {
            Name = $"{LauncherAssemblyInfo.ProductName}"
        };
        yield return launcherExecutable;
        yield return launcherUpdater;
        //yield return testExe;
    }


    private SingleFileComponent BuildFileComponent(IProductComponentIdentity identity, string name, string directoryKey, string fileName, string version, ProductVariables variables)
    {
        var installDirectory = EnsureVariable(variables, directoryKey);
        var filePath = _fileSystem.Path.Combine(installDirectory, fileName);
        return new SingleFileComponent(identity, installDirectory, fileName, null)
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

    private static string EnsureVariable(ProductVariables collection, string key)
    {
        var value = collection[key];
        if (string.IsNullOrEmpty(value))
            throw new InvalidOperationException($"Unable to resolve required variable: '{key}'");
        return value!;
    }
}