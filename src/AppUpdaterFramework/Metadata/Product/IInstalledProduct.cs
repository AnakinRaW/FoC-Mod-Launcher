using AnakinRaW.AppUpdaterFramework.Metadata.Component.Catalog;

namespace AnakinRaW.AppUpdaterFramework.Metadata.Product;

public interface IInstalledProduct : IProductReference
{ 
    string InstallationPath { get; }
    
    ProductVariables Variables { get; }

    ProductInstallState InstallState { get; }

    IProductManifest Manifest { get; }
}