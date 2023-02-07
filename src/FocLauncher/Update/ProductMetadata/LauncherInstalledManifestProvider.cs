using System;
using System.Collections.Generic;
using System.Linq;
using AnakinRaW.ProductMetadata;
using AnakinRaW.ProductMetadata.Catalog;
using AnakinRaW.ProductMetadata.Component;
using AnakinRaW.ProductMetadata.Conditions;
using AnakinRaW.ProductMetadata.Services;

namespace FocLauncher.Update.ProductMetadata;

internal class LauncherInstalledManifestProvider : IInstalledManifestProvider
{
    public IProductManifest ProvideManifest(IProductReference installedProduct, VariableCollection variables)
    {
        var components = BuildManifestComponents(installedProduct, variables).ToList();
        return new ProductManifest(installedProduct, components);
    }

    // Because we don't store an external manifest we have a few limitations:
    // a) We cannot add a HashCode or FileSize to this manifest, since it's not known at compile time.
    // b) For dependencies of this repository e.g., the AppUpdater we need to assume the same FileVersion as this assembly.
    private IEnumerable<IProductComponent> BuildManifestComponents(IProductReference installedProduct, VariableCollection variables)
    {
        var identityVersion = installedProduct.Version;
        var fileVersion = LauncherAssemblyInfo.FileVersion;

        var launcherExeId = new ProductComponentIdentity("Launcher.Executable", identityVersion);
        var launcherUpdaterId = new ProductComponentIdentity("Launcher.Updater", identityVersion);

        var currentDir = variables[KnownProductVariablesKeys.InstallDir];
        if (string.IsNullOrEmpty(currentDir))
            throw new InvalidOperationException(
                $"Unable to resolve required variable: '{KnownProductVariablesKeys.InstallDir}'");

        
        var launcherExecutable = new SingleFileComponent(launcherExeId, currentDir!, null, null)
        {
            DetectConditions = new []
            {
                new FileCondition($"[{KnownProductVariablesKeys.InstallDir}]FocLauncher.exe")
                {
                    Version = fileVersion
                }
            }
        };

        var launcherUpdater = new SingleFileComponent(launcherUpdaterId, currentDir!, null, null)
        {
            DetectConditions = new[]
            {
                new FileCondition($"[{KnownProductVariablesKeys.InstallDir}]FocLauncher.exe")
                {
                    Version = fileVersion
                }
            }
        };


        yield return new ComponentGroup(new ProductComponentIdentity("Launcher.CoreApplicationGroup", identityVersion), new List<IProductComponentIdentity>
        {
            launcherExeId,
            launcherUpdaterId
        });
        yield return launcherExecutable;
        yield return launcherUpdater;
    }
}