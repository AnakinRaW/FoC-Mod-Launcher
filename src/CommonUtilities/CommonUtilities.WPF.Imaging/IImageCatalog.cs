using System;
using System.Collections.Generic;

namespace AnakinRaW.CommonUtilities.Wpf.Imaging;

public interface IImageCatalog : IEnumerable<ImageDefinition>
{
    public Type CatalogType { get; }

    bool GetDefinition(string imageId, out ImageDefinition imageDefinition);
}