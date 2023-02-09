using AnakinRaW.AppUpaterFramework.Metadata.Component.Catalog;

namespace AnakinRaW.AppUpaterFramework.Metadata.Product;

public interface IInstalledProduct : IProductReference
{ 
    string InstallationPath { get; }
    
    ProductVariables Variables { get; }

    ProductInstallState InstallState { get; }

    IProductManifest Manifest { get; }
}