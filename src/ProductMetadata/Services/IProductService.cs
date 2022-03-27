using System;
using Sklavenwalker.ProductMetadata.Manifest;

namespace Sklavenwalker.ProductMetadata.Services;

public interface IProductService
{
    IInstalledProduct GetCurrentInstance();

    void UpdateCurrentInstance(IInstalledProduct product);

    IProductReference CreateProductReference(Version? newVersion, string? branch);

    IInstalledProductCatalog GetInstalledProductCatalog();

    IManifest GetAvailableProductManifest(ManifestLocation updateRequest);
}