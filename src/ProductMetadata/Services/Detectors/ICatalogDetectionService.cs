using System.Collections.Generic;
using AnakinRaW.ProductMetadata.Catalog;
using AnakinRaW.ProductMetadata.Component;

namespace AnakinRaW.ProductMetadata.Services.Detectors;

public interface ICatalogDetectionService
{
    void UpdateDetectionState(IProductCatalog catalog, VariableCollection? productVariables = null);

    void UpdateDetectionState(IReadOnlyCollection<IProductComponent> catalog, VariableCollection? productVariables = null);
}