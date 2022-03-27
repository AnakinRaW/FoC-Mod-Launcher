using System.Collections.Generic;
using Sklavenwalker.ProductMetadata.Component;

namespace Sklavenwalker.ProductMetadata;

public interface ICatalog
{
    IEnumerable<IProductComponentIdentity> Items { get; }
}

public interface ICatalog<T> : ICatalog where T : class, IProductComponentIdentity
{
    new IEnumerable<T> Items { get; }
}