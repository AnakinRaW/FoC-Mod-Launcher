using System;

namespace Sklavenwalker.ProductMetadata;

public interface IInstalledProduct : IProductReference
{ 
    string InstallationPath { get; }

    string? Author { get; }

    DateTime? UpdateDate { get; }

    DateTime? InstallDate { get; }

    ProductReleaseType ReleaseType { get; }

    VariableCollection ProductVariables { get; }
}