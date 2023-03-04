using System;

namespace AnakinRaW.AppUpdaterFramework.Metadata.Component.Catalog;

public class CatalogDownloadException : CatalogException
{
    public CatalogDownloadException(string message) : base(message)
    {
    }

    public CatalogDownloadException(string message, Exception inner) : base(message, inner)
    {
        
    }
}