using Sklavenwalker.CommonUtilities.Wpf.Imaging;

namespace FocLauncher.Imaging;

internal static class Monikers
{
    public static ImageMoniker Settings => new() { CatalogType = typeof(ImageCatalog), Name = "Settings" };
    public static ImageMoniker Undo => new() { CatalogType = typeof(ImageCatalog), Name = "Undo" };
    public static ImageMoniker Github => new() { CatalogType = typeof(ImageCatalog), Name = "Github" };
}