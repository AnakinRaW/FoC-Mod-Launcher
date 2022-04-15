using System.Collections.Generic;
using Validation;

namespace Sklavenwalker.ProductMetadata.Component;

public class ComponentGroup : ProductComponent, IComponentGroup
{
    public override ComponentType Type => ComponentType.Group;

    public ICollection<IProductComponent> Components { get; }

    public ComponentGroup(IProductComponentIdentity identity, ICollection<IProductComponent> components) : base(identity)
    {
        Requires.NotNull(components, nameof(components));
        Components = components;
    }
}