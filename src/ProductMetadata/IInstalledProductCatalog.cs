using Sklavenwalker.ProductMetadata.Component;

namespace Sklavenwalker.ProductMetadata;

public interface IInstalledProductCatalog : ICatalog<IProductComponent>
{
    IInstalledProduct Product { get; }
}