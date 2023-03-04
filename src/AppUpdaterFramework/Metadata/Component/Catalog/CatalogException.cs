using System;

namespace AnakinRaW.AppUpdaterFramework.Metadata.Component.Catalog;

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