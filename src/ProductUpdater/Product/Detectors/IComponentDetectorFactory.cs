using System;
using AnakinRaW.AppUpaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpaterFramework.Product.Detectors;

internal interface IComponentDetectorFactory
{
    IComponentDetector GetDetector(ComponentType type, IServiceProvider serviceProvider);
}