using System.Collections.Generic;
using Sklavenwalker.ProductMetadata;
using Sklavenwalker.ProductMetadata.Component;

namespace Sklavenwalker.ProductUpdater
{
    public interface IUpdateCatalog : ICatalog
    {
        IProductReference Product { get; }
        IEnumerable<ProductComponent> ComponentsToInstall { get; }
        IEnumerable<ProductComponent> ComponentsToKeep { get; }
        IEnumerable<ProductComponent> ComponentsToDelete { get; }
    }
}