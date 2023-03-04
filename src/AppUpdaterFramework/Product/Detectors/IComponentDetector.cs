using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;

namespace AnakinRaW.AppUpdaterFramework.Product.Detectors;

internal interface IComponentDetector
{
    bool GetCurrentInstalledState(IInstallableComponent installableComponent, ProductVariables productVariables);
}