using System;
using Sklavenwalker.ProductMetadata.Component;

namespace Sklavenwalker.ProductMetadata.Services.Detectors;

public interface IComponentDetectorFactory
{
    IComponentDetector GetDetector(ComponentType type, IServiceProvider serviceProvider);
}