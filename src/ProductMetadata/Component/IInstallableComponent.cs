using System.Collections.Generic;
using Sklavenwalker.ProductMetadata.Conditions;

namespace Sklavenwalker.ProductMetadata.Component;

public interface IInstallableComponent : IProductComponent
{
    long DownloadSize { get; }

    IList<OriginInfo> OriginInfos { get; }

    IList<ICondition> DetectConditions { get; }

    InstallSizes InstallSizes { get; }
}