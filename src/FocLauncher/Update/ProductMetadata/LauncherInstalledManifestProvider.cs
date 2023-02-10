﻿using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using AnakinRaW.AppUpaterFramework.Conditions;
using AnakinRaW.AppUpaterFramework.Metadata.Component;
using AnakinRaW.AppUpaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.AppUpaterFramework.Product.Manifest;
using Microsoft.Extensions.DependencyInjection;

namespace FocLauncher.Update.ProductMetadata;

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


        var launcherExecutable = BuildFileComponent(launcherExeId, KnownProductVariablesKeys.InstallDir,
            LauncherAssemblyInfo.AssemblyName, fileVersion, variables);
        var launcherUpdater = BuildFileComponent(launcherUpdaterId, LauncherVariablesKeys.LauncherAppData,
            LauncherConstants.AppUpdaterAssemblyName, fileVersion, variables);


        yield return new ComponentGroup(new ProductComponentIdentity("Launcher.CoreApplicationGroup", identityVersion), new List<IProductComponentIdentity>
        {
            launcherExeId,
            launcherUpdaterId
        });
        yield return launcherExecutable;
        yield return launcherUpdater;
    }


    private SingleFileComponent BuildFileComponent(IProductComponentIdentity identity, string directoryKey, string fileName, string version, ProductVariables variables)
    {
        var installDirectory = EnsureVariable(variables, directoryKey);
        var filePath = _fileSystem.Path.Combine(installDirectory, fileName);
        return new SingleFileComponent(identity, installDirectory, null)
        {
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