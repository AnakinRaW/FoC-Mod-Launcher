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

        public PreviewType UpdateSearchOption
        {
            get
            {
                LauncherRegistryHelper.GetValueOrDefault(LauncherRegistryKeys.UpdateSearchMode, out var value, PreviewType.Stable);
                return value;
            }
            set => LauncherRegistryHelper.WriteValue(LauncherRegistryKeys.UpdateSearchMode, value);
        }

        public PreviewType? CurrentUpdateSearchOption
        {
            get
            {
                LauncherRegistryHelper.GetValue<PreviewType?>(LauncherRegistryKeys.SessionUpdateSearchMode, out var value);
                return value;
            }
            set
            {
                if (value == null)
                    LauncherRegistryHelper.DeleteValue(LauncherRegistryKeys.SessionUpdateSearchMode);
                else
                    LauncherRegistryHelper.WriteValue(LauncherRegistryKeys.SessionUpdateSearchMode, value);
            }
        }

        public bool IsPreviewInstance
        {
            get
            {
                var current = CurrentUpdateSearchOption ?? UpdateSearchOption;
                return current != PreviewType.Stable;
            }
        }

        public static bool SuppressFallbackUpdate
        {
            get
            {
                LauncherRegistryHelper.GetValueOrDefault(LauncherRegistryKeys.SuppressFallbackUpdate, out var value, false);
                return value;
            }
            set => LauncherRegistryHelper.WriteValue(LauncherRegistryKeys.SuppressFallbackUpdate, value);
        }

        public UpdateMode UpdateMode
        {
            get
            {
                if (SuppressFallbackUpdate)
                    return UpdateMode.NoFallback;
                return CurrentUpdateSearchOption == null ? UpdateMode.FallbackStable : UpdateMode.Explicit;
            }
        }


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

    public enum UpdateMode
    {
        FallbackStable,
        Explicit,
        NoFallback,
    }
}
