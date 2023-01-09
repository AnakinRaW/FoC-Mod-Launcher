using System.Collections.Generic;

namespace AnakinRaW.ProductMetadata.Component;

public interface IComponentGroup : IProductComponent
{
    IReadOnlyList<IProductComponentIdentity> Components { get; } 
}