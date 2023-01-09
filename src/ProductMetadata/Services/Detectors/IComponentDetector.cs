using AnakinRaW.ProductMetadata.Component;

namespace AnakinRaW.ProductMetadata.Services.Detectors;

internal interface IComponentDetector
{
    bool GetCurrentInstalledState(IInstallableComponent installableComponent, VariableCollection productVariables);
}