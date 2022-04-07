using System;
using Sklavenwalker.ProductMetadata.Catalog;

namespace Sklavenwalker.ProductMetadata.Services;

public interface IProductService
{
    IInstalledProduct GetCurrentInstance();

    void UpdateCurrentInstance(IInstalledProduct product);

    IProductReference CreateProductReference(Version? newVersion, string? branch);

    IInstalledProductCatalog GetInstalledProductCatalog();

    IProductCatalog GetAvailableProductManifest(CatalogLocation updateRequest);
}