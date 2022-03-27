using System.Collections.Generic;

namespace Sklavenwalker.ProductMetadata.Component;

public interface IComponentGroup : IProductComponent
{
    IList<IProductComponent> Components { get; } 
}