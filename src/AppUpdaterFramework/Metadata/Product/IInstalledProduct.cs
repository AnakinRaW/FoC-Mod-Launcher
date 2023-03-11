using AnakinRaW.AppUpdaterFramework.Metadata.Component.Catalog;

namespace AnakinRaW.AppUpdaterFramework.Metadata.Product;

public interface IInstalledProduct : IProductReference
{ 
    string InstallationPath { get; }
    
    ProductVariables Variables { get; }

    ProductState State { get; }

    IProductManifest Manifest { get; }
}