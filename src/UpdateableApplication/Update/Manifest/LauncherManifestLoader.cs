using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Product;
using AnakinRaW.AppUpdaterFramework.Product.Manifest;
using Microsoft.Extensions.DependencyInjection;
using Semver;

namespace AnakinRaW.ApplicationBase.Update.Manifest;

internal class LauncherManifestLoader : ManifestLoaderBase
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public LauncherManifestLoader(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<IProductManifest> LoadManifestCore(Stream manifest, IProductReference productReference, CancellationToken cancellationToken)
    {
        var launcherManifest = await JsonSerializer.DeserializeAsync<LauncherManifest>(manifest, JsonSerializerOptions, cancellationToken);
        if (launcherManifest is null)
            throw new CatalogException("Serialized manifest is null");

        var availProduct = BuildReference(launcherManifest);
        ValidateCompatibleManifest(availProduct, productReference);
        var catalog = BuildCatalog(launcherManifest.Components);
        return new ProductManifest(availProduct, catalog);
    }

    private IProductReference BuildReference(LauncherManifest launcherManifest)
    {
        SemVersion? version = null;
        if (launcherManifest.Version is not null)
            version = SemVersion.Parse(launcherManifest.Version, SemVersionStyles.Any);

        ProductBranch? branch = null;
        if (version is not null && launcherManifest.Branch is not null)
        {
            var branchManager = ServiceProvider.GetRequiredService<IBranchManager>();
            branch = branchManager.GetBranchFromVersion(version);
        }
        return new ProductReference(launcherManifest.Name, version, branch);
    }

    private static IReadOnlyList<IProductComponent> BuildCatalog(IEnumerable<LauncherComponent> launcherManifestComponents)
    {
        var catalog = new List<IProductComponent>();
        foreach (var manifestComponent in launcherManifestComponents)
        {
            switch (manifestComponent.Type)
            {
                case ComponentType.File:
                    catalog.Add(manifestComponent.ToInstallable());
                    break;
                case ComponentType.Group:
                    catalog.Add(manifestComponent.ToGroup());
                    break;
                default:
                    throw new InvalidOperationException($"{manifestComponent.Type} is not supported.");
            }
        }
        return catalog;
    }
}