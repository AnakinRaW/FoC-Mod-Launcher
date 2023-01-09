using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.ProductMetadata.Catalog;

namespace AnakinRaW.ProductMetadata.Services;

public interface IManifestLoader
{
    Task<IProductCatalog> LoadManifest(IFileInfo manifestFile, IProductReference productReference, CancellationToken cancellationToken);

    Task<IProductCatalog> LoadManifest(Stream manifest, IProductReference productReference, CancellationToken cancellationToken, bool keepOpen = false);
}