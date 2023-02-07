using System;
using System.Collections.Generic;
using System.Linq;
using AnakinRaW.ProductMetadata.Catalog;
using AnakinRaW.ProductMetadata.Component;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace AnakinRaW.ProductMetadata.Services.Detectors;

internal class ManifestInstallationDetector : IManifestInstallationDetector
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IComponentDetectorFactory _componentDetectorFactory;

    public ManifestInstallationDetector(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _componentDetectorFactory = serviceProvider.GetService<IComponentDetectorFactory>() ?? new ComponentDetectorFactory();
    }

    public IReadOnlyCollection<IInstallableComponent> DetectInstalledComponents(IProductManifest manifest, VariableCollection? productVariables = null)
    {
        Requires.NotNull(manifest, nameof(manifest));
        productVariables ??= new VariableCollection();

        var installedComponents = new HashSet<IInstallableComponent>(ProductComponentIdentityComparer.VersionAndBranchIndependent);
        foreach (var manifestItem in manifest.Items)
        {
            if (manifestItem is not IInstallableComponent installable || manifestItem.DetectedState != DetectionState.None)
                continue;
            installable.DetectedState = IsInstalled(installable, productVariables);
            installedComponents.Add(installable);
        }
        return installedComponents.ToList();
    }

    private DetectionState IsInstalled(IInstallableComponent installable, VariableCollection productVariables)
    {
        var detector = _componentDetectorFactory.GetDetector(installable.Type, _serviceProvider);
        return detector.GetCurrentInstalledState(installable, productVariables) ? DetectionState.Present : DetectionState.Absent;
    }
}