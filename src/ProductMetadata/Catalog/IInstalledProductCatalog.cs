namespace AnakinRaW.ProductMetadata.Catalog;

public interface IInstalledProductCatalog : IProductCatalog
{
    new IInstalledProduct Product { get; }
}