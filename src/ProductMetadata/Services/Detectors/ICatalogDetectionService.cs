using System.Collections.Generic;
using AnakinRaW.ProductMetadata.Catalog;
using AnakinRaW.ProductMetadata.Component;

namespace AnakinRaW.ProductMetadata.Services.Detectors;

internal interface IManifestInstallationDetector
{
    IReadOnlyCollection<IInstallableComponent> DetectInstalledComponents(IProductManifest catalog, VariableCollection? productVariables = null);
}