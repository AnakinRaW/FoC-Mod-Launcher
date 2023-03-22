using System;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpdaterFramework.Product.Detectors;

internal interface IComponentDetectorFactory
{
    IComponentDetector GetDetector(ComponentType type, IServiceProvider serviceProvider);
}