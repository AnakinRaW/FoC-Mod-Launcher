using System.Collections.Generic;

namespace Sklavenwalker.ProductMetadata.Component;

public interface IComponentGroup : IProductComponent
{
    IReadOnlyList<IProductComponentIdentity> Components { get; } 
}