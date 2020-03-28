using FocLauncher;
using FocLauncherHost.Update.UpdateCatalog;
using TaskBasedUpdater;

namespace FocLauncherHost
{
    public class FocLauncherProduct : IProductInfo
    {
        private static FocLauncherProduct _instance;

        public string Name => LauncherConstants.ProductName;
        public string Author => LauncherConstants.Author;
        public string AppDataPath => LauncherConstants.ApplicationBasePath;
        public string CurrentLocation => GetType().Assembly.Location;

        // TODO: Decide how to get data
        public PreviewType PreviewType { get; }

        public bool IsPreviewInstance => PreviewType != PreviewType.None;

        public BuildType BuildType
        {
            get
            {
#if DEBUG
                return BuildType.Debug;
#else
                return BuildType.Release;
#endif
            }
        }

        public static FocLauncherProduct Instance => _instance ??= new FocLauncherProduct();

        private FocLauncherProduct()
        {
        }
    }
}
