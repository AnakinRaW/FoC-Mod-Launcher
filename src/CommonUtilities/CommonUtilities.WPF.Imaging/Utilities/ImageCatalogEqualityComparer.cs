using System.Collections.Generic;

namespace Sklavenwalker.CommonUtilities.Wpf.Imaging.Utilities;

internal class ImageCatalogEqualityComparer : EqualityComparer<IImageCatalog>
{
    public override bool Equals(IImageCatalog? x, IImageCatalog? y)
    {
        if (x == null || y == null)
            return false;
        return x.CatalogType == y.CatalogType;
    }

    public override int GetHashCode(IImageCatalog obj)
    {
        return obj.CatalogType.GetHashCode();
    }
}