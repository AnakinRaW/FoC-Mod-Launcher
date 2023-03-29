using System;
using System.Collections.Generic;
using AnakinRaW.AppUpdaterFramework.Conditions;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;

namespace AnakinRaW.AppUpdaterFramework.Metadata.Component;

public abstract class InstallableComponent : ProductComponent, IInstallableComponent
{
    public long DownloadSize => OriginInfo?.Size ?? 0;
    public OriginInfo? OriginInfo { get; }
    public IReadOnlyList<ICondition> DetectConditions { get; init; } = Array.Empty<ICondition>();
    public InstallationSize InstallationSize { get; init; }
   
    protected InstallableComponent(IProductComponentIdentity identity, OriginInfo? originInfo) 
        : base(identity)
    {
        OriginInfo = originInfo;
    }

    public abstract string? GetFullPath(IServiceProvider serviceProvider, ProductVariables? variables = null);
}