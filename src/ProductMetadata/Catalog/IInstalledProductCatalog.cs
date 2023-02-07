using AnakinRaW.ProductMetadata.Component;

namespace AnakinRaW.ProductMetadata.Catalog;

public interface IInstalledProductCatalog : IProductCatalog<IInstallableComponent>
{
    new IInstalledProduct Product { get; }
}