using System.Collections.Generic;
using AnakinRaW.AppUpaterFramework.Metadata.Product;

namespace AnakinRaW.AppUpaterFramework.Metadata.Component.Catalog;

public interface IProductCatalog<out T> where T : IProductComponent
{
    IProductReference Product { get; }

    IReadOnlyCollection<T> Items { get; }
}