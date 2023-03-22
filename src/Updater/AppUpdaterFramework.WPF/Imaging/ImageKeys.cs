using AnakinRaW.CommonUtilities.Wpf.Imaging;

namespace AnakinRaW.AppUpdaterFramework.Imaging;

public static class ImageKeys
{
    // Icons
    
    public static ImageKey StatusError => new() { CatalogType = typeof(ImageCatalog), Name = nameof(StatusError) };

    public static ImageKey StatusOK => new() { CatalogType = typeof(ImageCatalog), Name = nameof(StatusOK) };
}