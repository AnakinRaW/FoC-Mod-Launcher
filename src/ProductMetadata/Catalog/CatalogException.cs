using System;

namespace Sklavenwalker.ProductMetadata.Catalog;

public class CatalogException : Exception
{
    public CatalogException(string message)
        : base(message)
    {
    }

    public CatalogException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}