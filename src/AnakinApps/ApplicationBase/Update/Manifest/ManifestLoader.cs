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

internal class ManifestLoader : ManifestLoaderBase
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public ManifestLoader(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override async Task<IProductManifest> LoadManifestCore(Stream manifest, IProductReference productReference, CancellationToken cancellationToken)
    {
        var appManifest = await JsonSerializer.DeserializeAsync<UpdateManifest>(manifest, JsonSerializerOptions, cancellationToken);
        if (appManifest is null)
            throw new CatalogException("Serialized manifest is null");

        var availProduct = BuildReference(appManifest);
        ValidateCompatibleManifest(availProduct, productReference);
        var catalog = BuildCatalog(appManifest.Components);
        return new ProductManifest(availProduct, catalog);
    }

    private IProductReference BuildReference(UpdateManifest updateManifest)
    {
        SemVersion? version = null;
        if (updateManifest.Version is not null)
            version = SemVersion.Parse(updateManifest.Version, SemVersionStyles.Any);

        ProductBranch? branch = null;
        if (version is not null && updateManifest.Branch is not null)
        {
            var branchManager = ServiceProvider.GetRequiredService<IBranchManager>();
            branch = branchManager.GetBranchFromVersion(version);
        }
        return new ProductReference(updateManifest.Name, version, branch);
    }

    private static IReadOnlyList<IProductComponent> BuildCatalog(IEnumerable<AppComponent> manifestComponents)
    {
        var catalog = new List<IProductComponent>();
        foreach (var manifestComponent in manifestComponents)
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