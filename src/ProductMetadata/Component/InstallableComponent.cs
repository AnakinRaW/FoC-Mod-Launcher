using System;
using System.Collections.Generic;
using AnakinRaW.ProductMetadata.Conditions;
using Validation;

namespace AnakinRaW.ProductMetadata.Component;

public abstract class InstallableComponent : ProductComponent, IInstallableComponent
{
    public long DownloadSize => OriginInfo?.Size ?? 0;
    public OriginInfo? OriginInfo { get; }
    public IReadOnlyList<ICondition> DetectConditions { get; init; } = Array.Empty<ICondition>();
    public InstallationSize InstallationSize { get; init; }
        
    protected InstallableComponent(IProductComponentIdentity identity, OriginInfo originInfo) 
        : base(identity)
    {
        OriginInfo = originInfo;
    }
}