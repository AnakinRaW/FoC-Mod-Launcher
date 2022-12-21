using System.Collections.Generic;
using Sklavenwalker.ProductMetadata.Conditions;

namespace Sklavenwalker.ProductMetadata.Component;

public interface IInstallableComponent : IProductComponent
{
    long DownloadSize { get; }

    OriginInfo OriginInfo { get; }

    IReadOnlyList<ICondition> DetectConditions { get; }

    InstallationSize InstallationSize { get; }
}