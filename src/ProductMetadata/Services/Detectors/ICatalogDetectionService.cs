using Sklavenwalker.ProductMetadata.Catalog;

namespace Sklavenwalker.ProductMetadata.Services.Detectors;

public interface ICatalogDetectionService
{
    void UpdateDetectionState(IProductCatalog catalog, VariableCollection productVariables, bool forceInvalidate = false);
}