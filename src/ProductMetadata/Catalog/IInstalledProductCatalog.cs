namespace Sklavenwalker.ProductMetadata.Catalog;

public interface IInstalledProductCatalog : IProductCatalog
{
    new IInstalledProduct Product { get; }
}