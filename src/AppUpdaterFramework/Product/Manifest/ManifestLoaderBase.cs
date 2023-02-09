using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.AppUpaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using Validation;

namespace AnakinRaW.AppUpaterFramework.Product.Manifest;

public abstract class ManifestLoaderBase : IManifestLoader
{
    protected readonly IServiceProvider ServiceProvider;

    protected ManifestLoaderBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public async Task<IProductManifest> LoadManifest(IFileInfo manifestFile, IProductReference productReference, CancellationToken cancellationToken)
    {
        Requires.NotNull(manifestFile, nameof(manifestFile));
        Requires.NotNull(productReference, nameof(productReference));
        using var manifest = manifestFile.OpenRead();
        return await LoadManifestCore(manifest, productReference, cancellationToken);
    }

    public async Task<IProductManifest> LoadManifest(Stream manifest, IProductReference productReference, CancellationToken cancellationToken, bool keepOpen = false)
    {
        Requires.NotNull(manifest, nameof(manifest));
        Requires.NotNull(productReference, nameof(productReference));
        try
        {
            return await LoadManifestCore(manifest, productReference, cancellationToken);
        }
        finally
        {
            if (!keepOpen)
                manifest.Close();
        }
    }

    protected abstract Task<IProductManifest> LoadManifestCore(Stream manifest, IProductReference productReference, CancellationToken cancellationToken);

    protected void ValidateCompatibleManifest(IProductReference manifestProduct, IProductReference installedProduct)
    {
        if (!ProductReferenceEqualityComparer.NameOnly.Equals(manifestProduct, installedProduct))
            throw new CatalogException(
                $"Manifest for '{manifestProduct.Name}' does not match installed product '{installedProduct.Name}' by name.");
    }
}