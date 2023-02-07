using AnakinRaW.ProductMetadata.Catalog;

namespace AnakinRaW.ProductMetadata;

public interface IInstalledProduct : IProductReference
{ 
    string InstallationPath { get; }
    
    VariableCollection ProductVariables { get; }

    ProductInstallState InstallState { get; }

    IProductManifest Manifest { get; }
}