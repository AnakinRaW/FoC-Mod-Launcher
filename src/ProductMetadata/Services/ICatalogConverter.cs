using Sklavenwalker.ProductMetadata.Catalog;

namespace Sklavenwalker.ProductMetadata.Services;

public interface ICatalogConverter<in TCatalogModel>
{
    public IProductCatalog Convert(TCatalogModel catalogModel);
}