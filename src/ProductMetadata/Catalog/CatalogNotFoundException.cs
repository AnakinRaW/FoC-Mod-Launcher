namespace Sklavenwalker.ProductMetadata.Catalog;

public class CatalogNotFoundException : CatalogException
{
    public CatalogNotFoundException(string message) : base(message)
    {
    }
}