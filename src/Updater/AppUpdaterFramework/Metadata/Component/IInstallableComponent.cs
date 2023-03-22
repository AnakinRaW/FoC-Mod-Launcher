using System.Collections.Generic;
using AnakinRaW.AppUpdaterFramework.Conditions;

namespace AnakinRaW.AppUpdaterFramework.Metadata.Component;

public interface IInstallableComponent : IProductComponent
{
    long DownloadSize { get; }

    OriginInfo? OriginInfo { get; }

    IReadOnlyList<ICondition> DetectConditions { get; }

    InstallationSize InstallationSize { get; }
}