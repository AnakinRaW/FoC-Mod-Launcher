using System;
using Sklavenwalker.ProductMetadata.Catalog;

namespace Sklavenwalker.ProductMetadata;

public interface IInstalledProduct : IProductReference
{ 
    string InstallationPath { get; }

    IProductCatalog Manifest { get; }
        
    string? Author { get; }

    DateTime? UpdateDate { get; }

    DateTime? InstallDate { get; }

    ProductReleaseType ReleaseType { get; }

    VariableCollection ProductVariables { get; }
}