using System.Collections.Generic;
using AnakinRaW.AppUpaterFramework.Metadata.Component;
using AnakinRaW.AppUpaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpaterFramework.Metadata.Product;

namespace AnakinRaW.AppUpaterFramework.Product.Manifest;

public interface IManifestInstallationDetector
{
    IReadOnlyCollection<IInstallableComponent> DetectInstalledComponents(IProductManifest catalog, ProductVariables? productVariables = null);
}