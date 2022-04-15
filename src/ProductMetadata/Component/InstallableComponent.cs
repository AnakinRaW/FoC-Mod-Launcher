using System.Collections.Generic;
using System.Linq;
using Sklavenwalker.ProductMetadata.Conditions;

namespace Sklavenwalker.ProductMetadata.Component;

public abstract class InstallableComponent : ProductComponent, IInstallableComponent
{
    public long DownloadSize => !OriginInfos.Any() ? 0 : OriginInfos.Sum(x => x.Size ?? 0);
    public IList<OriginInfo> OriginInfos { get; init; } = new List<OriginInfo>();
    public IList<ICondition> DetectConditions { get; init; } = new List<ICondition>();
    public InstallSizes InstallSizes { get; init; }
        
    protected InstallableComponent(IProductComponentIdentity identity) 
        : base(identity)
    {
    }
}