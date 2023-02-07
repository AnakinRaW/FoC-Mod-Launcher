using System.Collections.Generic;
using System.Linq;
using AnakinRaW.ProductMetadata;
using AnakinRaW.ProductMetadata.Catalog;
using AnakinRaW.ProductMetadata.Component;
using AnakinRaW.ProductMetadata.Services;

namespace FocLauncher.Update.ProductMetadata;

internal class LauncherInstalledManifestProvider : IInstalledManifestProvider
{
    private static readonly ICollection<(string componentName, string location)> ExpectedComponents =
        new List<(string componentName, string location)>
        {

        };

    // Because we don't store an external manifest we have a few limitations:
    // a) We cannot add a HashCode or FileSize to this manifest, since it's not known at compile time.
    // b) For dependencies of this repository e.g., the AppUpdater we need to assume the same FileVersion as this assembly.
    public IProductManifest ProvideManifest(IProductReference installedProduct, VariableCollection variables)
    {
        var components = BuildManifestComponents(installedProduct, variables).ToList();
        return new ProductManifest(installedProduct, components);
    }

    private IEnumerable<IProductComponent> BuildManifestComponents(IProductReference installedProduct, VariableCollection variables)
    {
        yield break;
    }
}