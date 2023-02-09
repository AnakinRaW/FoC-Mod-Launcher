using AnakinRaW.CommonUtilities.Wpf.Imaging;

namespace FocLauncher.Imaging;

internal static class ImageKeys
{
    public static ImageKey Settings => new() { CatalogType = typeof(ImageCatalog), Name = "Settings" };
    public static ImageKey Undo => new() { CatalogType = typeof(ImageCatalog), Name = "Undo" };
    public static ImageKey Github => new() { CatalogType = typeof(ImageCatalog), Name = "Github" };

    public static ImageKey Trooper => new() { CatalogType = typeof(ImageCatalog), Name = "Trooper" };

    public static ImageKey UpdateIcon => new() { CatalogType = typeof(ImageCatalog), Name = nameof(UpdateIcon) };

    public static ImageKey StatusHelpIcon => new() { CatalogType = typeof(ImageCatalog), Name = nameof(StatusHelpIcon) };

    public static ImageKey AppIcon => new() { CatalogType = typeof(ImageCatalog), Name = nameof(AppIcon) };

    public static ImageKey StatusError => new() { CatalogType = typeof(ImageCatalog), Name = nameof(StatusError) };
}