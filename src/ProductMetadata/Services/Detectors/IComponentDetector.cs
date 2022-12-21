using Sklavenwalker.ProductMetadata.Component;

namespace Sklavenwalker.ProductMetadata.Services.Detectors;

internal interface IComponentDetector
{
    bool GetCurrentInstalledState(IInstallableComponent installableComponent, VariableCollection productVariables);
}