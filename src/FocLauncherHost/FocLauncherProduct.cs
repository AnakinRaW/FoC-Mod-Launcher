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

        public PreviewType UpdateSearchOption => GetPreviewMode();
        
        public PreviewType? CurrentUpdateSearchOption => GetCurrentPreviewMode();
        
        public bool IsPreviewInstance => UpdateSearchOption != PreviewType.Stable;

        public UpdateMode UpdateMode => CurrentUpdateSearchOption == null ? UpdateMode.FallbackStable : UpdateMode.Explicit;


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

        private static PreviewType GetPreviewMode()
        {
            var initialValue =
                LauncherRegistryHelper.GetValueOrDefault(LauncherRegistryKeys.UpdateSearchMode, PreviewType.Stable);

            return initialValue;
        }

        private PreviewType GetCurrentPreviewMode()
        {
            var initialValue =
                LauncherRegistryHelper.GetValueOrDefault(LauncherRegistryKeys.SessionUpdateSearchMode, PreviewType.Stable);

            return initialValue;
        }
    }

    public enum UpdateMode
    {
        FallbackStable,
        Explicit
    }
}
