using System;
using System.Collections.Generic;
using AnakinRaW.ProductMetadata.Catalog;
using AnakinRaW.ProductMetadata.Component;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace AnakinRaW.ProductMetadata.Services.Detectors;

public class CatalogDetectionService : ICatalogDetectionService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IComponentDetectorFactory _componentDetectorFactory;

    public CatalogDetectionService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _componentDetectorFactory = serviceProvider.GetService<IComponentDetectorFactory>() ?? new ComponentDetectorFactory();
    }

    public void UpdateDetectionState(IProductCatalog catalog, VariableCollection? productVariables = null)
    {
        Requires.NotNull(catalog, nameof(catalog));
        UpdateDetectionState(catalog.Items);
    }

    public void UpdateDetectionState(IReadOnlyCollection<IProductComponent> catalog, VariableCollection? productVariables = null)
    {
        Requires.NotNull(catalog, nameof(catalog));
        productVariables ??= new VariableCollection();
        foreach (var component in catalog)
        {
            if (component is not IInstallableComponent installable || component.DetectedState != DetectionState.None)
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