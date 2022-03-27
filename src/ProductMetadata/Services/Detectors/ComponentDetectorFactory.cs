using System;
using System.Collections.Generic;
using Sklavenwalker.ProductMetadata.Component;
using Validation;

namespace Sklavenwalker.ProductMetadata.Services.Detectors;

internal class ComponentDetectorFactory : IComponentDetectorFactory
{
    private readonly Dictionary<ComponentType, IComponentDetector> _detectors = new();

    public IComponentDetector GetDetector(ComponentType type, IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        switch (type)
        {
            case ComponentType.File:
                return GetOrCreate(type, () => new FileComponentDetector(serviceProvider));
            case ComponentType.Group:
                return GetOrCreate(type, () => new GroupComponentDetector(serviceProvider));
            default:
                throw new NotSupportedException();
        }
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