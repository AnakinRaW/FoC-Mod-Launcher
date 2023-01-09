using System;
using AnakinRaW.ProductMetadata.Component;

namespace AnakinRaW.ProductMetadata.Services.Detectors;

internal interface IComponentDetectorFactory
{
    IComponentDetector GetDetector(ComponentType type, IServiceProvider serviceProvider);
}