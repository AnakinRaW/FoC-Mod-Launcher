using System.Collections.Generic;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;

namespace AnakinRaW.AppUpdaterFramework.Product.Manifest;

public interface IManifestInstallationDetector
{
    IReadOnlyCollection<IInstallableComponent> DetectInstalledComponents(IProductManifest catalog, ProductVariables? productVariables = null);
}