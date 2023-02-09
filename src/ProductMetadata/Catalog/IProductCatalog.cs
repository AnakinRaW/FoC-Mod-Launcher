using System.Collections.Generic;
using AnakinRaW.ProductMetadata.Component;

namespace AnakinRaW.ProductMetadata.Catalog;

public interface IProductCatalog<out T> where T : IProductComponent
{
    IProductReference Product { get; }

    IReadOnlyCollection<T> Items { get; }
}