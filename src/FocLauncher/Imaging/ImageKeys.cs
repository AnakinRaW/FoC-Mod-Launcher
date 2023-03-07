using AnakinRaW.CommonUtilities.Wpf.Imaging;

namespace FocLauncher.Imaging;

internal static class ImageKeys
{
    public static ImageKey Settings => new() { CatalogType = typeof(ImageCatalog), Name = "Settings" };
    public static ImageKey Undo => new() { CatalogType = typeof(ImageCatalog), Name = "Undo" };
    public static ImageKey Github => new() { CatalogType = typeof(ImageCatalog), Name = "Github" };

    public static ImageKey Trooper => new() { CatalogType = typeof(ImageCatalog), Name = nameof(Trooper) };

    public static ImageKey Vader => new() { CatalogType = typeof(ImageCatalog), Name = nameof(Vader) };

    public static ImageKey SwPulp => new() { CatalogType = typeof(ImageCatalog), Name = nameof(SwPulp) };

    public static ImageKey Palpatine => new() { CatalogType = typeof(ImageCatalog), Name = nameof(Palpatine) };

    public static ImageKey UpdateIcon => new() { CatalogType = typeof(ImageCatalog), Name = nameof(UpdateIcon) };

    public static ImageKey StatusHelpIcon => new() { CatalogType = typeof(ImageCatalog), Name = nameof(StatusHelpIcon) };

    public static ImageKey AppIcon => new() { CatalogType = typeof(ImageCatalog), Name = nameof(AppIcon) };

    public static ImageKey StatusError => new() { CatalogType = typeof(ImageCatalog), Name = nameof(StatusError) };

    public static ImageKey StatusOK => new() { CatalogType = typeof(ImageCatalog), Name = nameof(StatusOK) };
}