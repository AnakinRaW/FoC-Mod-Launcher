using System.Collections.Generic;
using System.Linq;
using Sklavenwalker.ProductMetadata;
using Sklavenwalker.ProductMetadata.Component;
using Validation;

namespace Sklavenwalker.ProductUpdater
{
    public class UpdateCatalog : Catalog, IUpdateCatalog
    {
        public IProductReference Product { get; }
        public IEnumerable<ProductComponent> ComponentsToInstall => Items.Where(x => x.RequiredAction == ComponentAction.Update);
        public IEnumerable<ProductComponent> ComponentsToKeep => Items.Where(x => x.RequiredAction == ComponentAction.Keep);
        public IEnumerable<ProductComponent> ComponentsToDelete => Items.Where(x => x.RequiredAction == ComponentAction.Delete);

        public UpdateCatalog(IProductReference product, IEnumerable<ProductComponent> components) : base(components)
        {
            Requires.NotNull(product, nameof(product));
            Product = product;
        }
    }
}