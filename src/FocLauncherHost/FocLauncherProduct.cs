using System.ComponentModel;
using System.Runtime.CompilerServices;
using FocLauncher;
using FocLauncherHost.Annotations;
using FocLauncherHost.Update.UpdateCatalog;
using TaskBasedUpdater;

namespace FocLauncherHost
{
    public class FocLauncherProduct : IProductInfo, INotifyPropertyChanged
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
            set
            {
                LauncherRegistryHelper.WriteValue(LauncherRegistryKeys.UpdateSearchMode, value);
                OnPropertyChanged();
            }
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
                OnPropertyChanged();
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

        public bool SuppressFallbackUpdate
        {
            get
            {
                LauncherRegistryHelper.GetValueOrDefault(LauncherRegistryKeys.SuppressFallbackUpdate, out var value, false);
                return value;
            }
            set
            {
                LauncherRegistryHelper.WriteValue(LauncherRegistryKeys.SuppressFallbackUpdate, value);
                OnPropertyChanged();
            }
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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum UpdateMode
    {
        FallbackStable,
        Explicit,
        NoFallback,
    }
}
