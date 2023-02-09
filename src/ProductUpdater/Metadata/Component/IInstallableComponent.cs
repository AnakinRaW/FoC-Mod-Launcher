using System.Collections.Generic;
using AnakinRaW.AppUpaterFramework.Conditions;

namespace AnakinRaW.AppUpaterFramework.Metadata.Component;

public interface IInstallableComponent : IProductComponent
{
    long DownloadSize { get; }

    OriginInfo? OriginInfo { get; }

    IReadOnlyList<ICondition> DetectConditions { get; }

    InstallationSize InstallationSize { get; }
}