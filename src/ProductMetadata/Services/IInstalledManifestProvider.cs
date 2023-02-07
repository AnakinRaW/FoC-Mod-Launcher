using AnakinRaW.ProductMetadata.Catalog;

namespace AnakinRaW.ProductMetadata.Services;

public interface IInstalledManifestProvider
{
    IProductManifest ProvideManifest(IProductReference installedProduct, VariableCollection variableCollection);
}