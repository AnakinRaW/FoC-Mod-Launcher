using System;
using Sklavenwalker.ProductMetadata.Manifest;

namespace Sklavenwalker.ProductMetadata;

public interface IInstalledProduct : IProductReference
{ 
    string InstallationPath { get; }

    IManifest CurrentManifest { get; }
        
    string? Author { get; }

    DateTime? UpdateDate { get; }

    DateTime? InstallDate { get; }

    ProductReleaseType ReleaseType { get; }

    VariableCollection ProductVariables { get; }
}