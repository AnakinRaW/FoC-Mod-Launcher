using AnakinRaW.AppUpaterFramework.Metadata.Component;
using AnakinRaW.AppUpaterFramework.Metadata.Product;

namespace AnakinRaW.AppUpaterFramework.Product.Detectors;

internal interface IComponentDetector
{
    bool GetCurrentInstalledState(IInstallableComponent installableComponent, ProductVariables productVariables);
}