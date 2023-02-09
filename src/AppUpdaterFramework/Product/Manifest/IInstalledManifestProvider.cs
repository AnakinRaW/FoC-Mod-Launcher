using AnakinRaW.AppUpaterFramework.Metadata.Component.Catalog;
using AnakinRaW.AppUpaterFramework.Metadata.Product;

namespace AnakinRaW.AppUpaterFramework.Product.Manifest;

public interface IInstalledManifestProvider
{
    IProductManifest ProvideManifest(IProductReference installedProduct, ProductVariables variableCollection);
}