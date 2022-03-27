using System.Collections.Generic;
using Sklavenwalker.ProductMetadata.Component;
using Validation;

namespace Sklavenwalker.ProductMetadata;

public class Catalog : ICatalog<IProductComponent>
{
    public IEnumerable<IProductComponent> Items { get; }

    public Catalog(IEnumerable<IProductComponent> components)
    {
        Requires.NotNull(components, nameof(components));
        Items = components;
    }

    IEnumerable<IProductComponentIdentity> ICatalog.Items => Items;
}