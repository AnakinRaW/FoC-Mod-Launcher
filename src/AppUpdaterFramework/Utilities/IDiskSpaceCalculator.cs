using AnakinRaW.AppUpdaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpdaterFramework.Utilities;

internal interface IDiskSpaceCalculator
{
    void ThrowIfNotEnoughDiskSpaceAvailable(
        IInstallableComponent newComponent, 
        IInstallableComponent? oldComponent,
        string? installPath, 
        DiskSpaceCalculator.CalculationOptions options);
}