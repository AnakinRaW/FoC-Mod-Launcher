using System;
using Sklavenwalker.ProductMetadata.Component;

namespace Sklavenwalker.ProductMetadata.Services.Detectors;

internal interface IComponentDetectorFactory
{
    IComponentDetector GetDetector(ComponentType type, IServiceProvider serviceProvider);
}