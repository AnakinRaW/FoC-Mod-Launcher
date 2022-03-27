using Sklavenwalker.ProductMetadata.Component;

namespace Sklavenwalker.ProductMetadata.Manifest;

public interface IManifest : ICatalog<IProductComponent>
{
    IProductReference Product { get; }
}