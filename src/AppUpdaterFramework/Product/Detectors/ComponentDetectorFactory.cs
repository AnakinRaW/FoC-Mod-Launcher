using System;
using System.Collections.Generic;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.Product.Detectors;

internal class ComponentDetectorFactory : IComponentDetectorFactory
{
    internal static readonly IComponentDetectorFactory Default = new ComponentDetectorFactory();

    private readonly Dictionary<ComponentType, IComponentDetector> _detectors = new();

    public IComponentDetector GetDetector(ComponentType type, IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        return type switch
        {
            ComponentType.File => GetOrCreate(type, () => new DefaultComponentDetector(serviceProvider)),
            _ => throw new NotSupportedException()
        };
    }

    private IComponentDetector GetOrCreate(ComponentType type, Func<IComponentDetector> createFunc)
    {
        Requires.NotNull(createFunc, nameof(createFunc));
        if (!_detectors.TryGetValue(type, out var detector))
        {
            detector = createFunc();
            Assumes.NotNull(detector);
            _detectors[type] = detector;
        }
        return detector;
    }
}