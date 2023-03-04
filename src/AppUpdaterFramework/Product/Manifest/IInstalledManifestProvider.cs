using AnakinRaW.AppUpdaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;

namespace AnakinRaW.AppUpdaterFramework.Product.Manifest;

public interface IInstalledManifestProvider
{
    IProductManifest ProvideManifest(IProductReference installedProduct, ProductVariables variableCollection);
}