using AnakinRaW.ProductMetadata.Catalog;

namespace AnakinRaW.ProductMetadata.Services;

public interface ICatalogConverter<in TCatalogModel>
{
    public IProductCatalog Convert(TCatalogModel catalogModel);
}