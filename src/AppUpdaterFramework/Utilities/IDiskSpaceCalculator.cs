using AnakinRaW.AppUpaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpaterFramework.Utilities;

internal interface IDiskSpaceCalculator
{
    void ThrowIfNotEnoughDiskSpaceAvailable(
        IInstallableComponent newComponent, 
        IInstallableComponent? oldComponent,
        string? installPath, 
        DiskSpaceCalculator.CalculationOptions options);
}