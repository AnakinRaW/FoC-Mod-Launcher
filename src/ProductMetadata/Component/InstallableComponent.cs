using System;
using System.Collections.Generic;
using Sklavenwalker.ProductMetadata.Conditions;
using Validation;

namespace Sklavenwalker.ProductMetadata.Component;

public abstract class InstallableComponent : ProductComponent, IInstallableComponent
{
    public long DownloadSize => OriginInfo.Size ?? 0;
    public OriginInfo OriginInfo { get; }
    public IReadOnlyList<ICondition> DetectConditions { get; init; } = Array.Empty<ICondition>();
    public InstallationSize InstallationSize { get; init; }
        
    protected InstallableComponent(IProductComponentIdentity identity, OriginInfo originInfo) 
        : base(identity)
    {
        Requires.NotNull(originInfo, nameof(originInfo));
        OriginInfo = originInfo;
    }
}