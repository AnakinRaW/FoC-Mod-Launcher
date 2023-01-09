using System.IO.Abstractions;
using AnakinRaW.ProductMetadata.Catalog;

namespace AnakinRaW.ProductMetadata.Services;

public interface ICatalogBuilder
{
    IProductCatalog Build(IFileInfo manifestFile, IProductReference product);
}