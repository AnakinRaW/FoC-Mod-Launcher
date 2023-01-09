using AnakinRaW.ProductMetadata.Catalog;

namespace AnakinRaW.ProductMetadata.Services.Detectors;

public interface ICatalogDetectionService
{
    void UpdateDetectionState(IProductCatalog catalog, VariableCollection productVariables, bool forceInvalidate = false);
}