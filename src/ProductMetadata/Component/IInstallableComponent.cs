using System.Collections.Generic;

namespace Sklavenwalker.ProductMetadata.Component;

public interface IInstallableComponent
{
    long DownloadSize { get; }

    IList<OriginInfo> OriginInfos { get; }

    IList<Condition> DetectConditions { get; }

    InstallSizes InstallSizes { get; }
}