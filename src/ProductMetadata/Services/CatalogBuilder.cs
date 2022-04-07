using System.IO;
using System.IO.Abstractions;
using Sklavenwalker.ProductMetadata.Catalog;
using Validation;

namespace Sklavenwalker.ProductMetadata.Services;

public abstract class CatalogBuilder<T> : ICatalogBuilder where T: notnull
{
    public IProductCatalog Build(IFileInfo manifestFile, IProductReference product)
    {
        Requires.NotNull(manifestFile, nameof(manifestFile));
        Requires.NotNull(product, nameof(product));
        var manifestModel = SerializeFile(manifestFile, product);
        if (manifestModel is null)
            throw new CatalogException($"Failed to get manifest from '{manifestFile.FullName}'.");
        return Build(manifestModel, product);
    }

    protected abstract IProductCatalog Build(T manifestModel, IProductReference product);

    protected abstract T SerializeModel(Stream manifestData, IProductReference productReference);

    private T SerializeFile(IFileInfo manifestFile, IProductReference product)
    {
        using var fileStream = manifestFile.OpenRead();
        return SerializeModel(fileStream, product);
    }
}