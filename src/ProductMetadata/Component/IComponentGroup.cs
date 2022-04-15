using System.Collections.Generic;

namespace Sklavenwalker.ProductMetadata.Component;

public interface IComponentGroup : IProductComponent
{
    ICollection<IProductComponent> Components { get; } 
}