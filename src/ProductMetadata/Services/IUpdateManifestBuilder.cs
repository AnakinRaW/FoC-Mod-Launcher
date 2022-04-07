using System.IO.Abstractions;
using Sklavenwalker.ProductMetadata.Catalog;

namespace Sklavenwalker.ProductMetadata.Services;

public interface ICatalogBuilder
{
    IProductCatalog Build(IFileInfo manifestFile, IProductReference product);
}