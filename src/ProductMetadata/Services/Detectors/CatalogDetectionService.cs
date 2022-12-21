using System;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.ProductMetadata.Catalog;
using Sklavenwalker.ProductMetadata.Component;
using Validation;

namespace Sklavenwalker.ProductMetadata.Services.Detectors;

public class CatalogDetectionService : ICatalogDetectionService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IComponentDetectorFactory _componentDetectorFactory;

    public CatalogDetectionService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _componentDetectorFactory = serviceProvider.GetService<IComponentDetectorFactory>() ?? new ComponentDetectorFactory();
    }

    public void UpdateDetectionState(IProductCatalog catalog, VariableCollection productVariables, bool forceInvalidate = false)
    {
        Requires.NotNull(catalog, nameof(catalog));
        Requires.NotNull(productVariables, nameof(productVariables));
        foreach (var component in catalog.Items)
        {
            if (forceInvalidate || component is not IInstallableComponent installable || component.DetectedState != DetectionState.None)
                continue;
            var installed = IsInstalled(installable, productVariables);
            installable.DetectedState = installed ? DetectionState.Present : DetectionState.Absent;
        }
    }

    internal bool IsInstalled(IInstallableComponent installable, VariableCollection productVariables)
    {
        var detector = _componentDetectorFactory.GetDetector(installable.Type, _serviceProvider);
        return detector.GetCurrentInstalledState(installable, productVariables);
    }
}