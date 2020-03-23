using FocLauncher;
using TaskBasedUpdater;

namespace FocLauncherHost
{
    internal class FocLauncherProduct : IProductInfo
    {
        private static FocLauncherProduct _instance;

        public string Name { get; }
        public string Author => LauncherConstants.Author;
        public string AppDataPath => LauncherConstants.ApplicationBasePath;
        public string CurrentLocation => GetType().Assembly.Location;

        public PreviewType PreviewType { get; private set; }

        // TODO: Decide how to get data
        public bool IsPreviewInstance { get; } = true;

        public bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public static FocLauncherProduct Instance => _instance ??= new FocLauncherProduct();

        private FocLauncherProduct()
        {
            Name = GetProductName();
        }

        private string GetProductName()
        {
            var name = LauncherConstants.ProductName;
            if (IsPreviewInstance && PreviewType != PreviewType.None)
                name = $"{name}-{PreviewType}";
            if (IsDebug)
                name = $"{name}-Debug";
            return name;
        }
    }

    internal enum PreviewType
    {
        None,
        Beta,
        Alpha
    }
}
