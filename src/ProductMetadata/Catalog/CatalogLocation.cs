using System;
using Validation;

namespace Sklavenwalker.ProductMetadata.Catalog;

public sealed class CatalogLocation
{
    public Uri ManifestUri { get; }

    public IProductReference Product { get; }

    public CatalogLocation(IProductReference product, Uri manifestUri)
    {
        Requires.NotNull(product, nameof(product));
        Requires.NotNull(manifestUri, nameof(manifestUri));
        Product = product;
        ManifestUri = manifestUri;
    }
}