using System.Collections.Generic;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.Metadata.Component;

public class ComponentGroup : ProductComponent, IComponentGroup
{
    public override ComponentType Type => ComponentType.Group;

    public IReadOnlyList<IProductComponentIdentity> Components { get; }

    public ComponentGroup(IProductComponentIdentity identity, IReadOnlyList<IProductComponentIdentity> components) : base(identity)
    {
        Requires.NotNull(components, nameof(components));
        Components = components;
    }
}