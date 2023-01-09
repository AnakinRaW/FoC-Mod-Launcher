using System.Collections.Generic;
using AnakinRaW.ProductMetadata.Conditions;

namespace AnakinRaW.ProductMetadata.Component;

public interface IInstallableComponent : IProductComponent
{
    long DownloadSize { get; }

    OriginInfo OriginInfo { get; }

    IReadOnlyList<ICondition> DetectConditions { get; }

    InstallationSize InstallationSize { get; }
}