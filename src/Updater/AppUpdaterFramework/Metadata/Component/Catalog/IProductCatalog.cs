using System.Collections.Generic;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;

namespace AnakinRaW.AppUpdaterFramework.Metadata.Component.Catalog;

public interface IProductCatalog<out T> where T : IProductComponent
{
    IProductReference Product { get; }

    IReadOnlyCollection<T> Items { get; }
}