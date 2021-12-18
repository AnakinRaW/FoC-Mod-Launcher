using System.Collections.Generic;

namespace Sklavenwalker.CommonUtilities.Wpf.Imaging;

internal class ImageCatalogEqualityComparer : EqualityComparer<IImageCatalog>
{
    public override bool Equals(IImageCatalog x, IImageCatalog y)
    {
        return x.CatalogType == y.CatalogType;
    }

    public override int GetHashCode(IImageCatalog obj)
    {
        return obj.CatalogType.GetHashCode();
    }
}