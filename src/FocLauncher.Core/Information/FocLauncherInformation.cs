using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FocLauncher
{
    public class FocLauncherInformation : INotifyPropertyChanged
    {
        private static FocLauncherInformation _instance;

        public string Name => LauncherConstants.ProductName;
        public string Author => LauncherConstants.Author;
        public string AppDataPath => LauncherConstants.ApplicationBasePath;
        public string CurrentLocation => GetType().Assembly.Location;

        public ApplicationType UpdateSearchOption
        {
            get
            {
                LauncherRegistryHelper.GetValueOrDefault(LauncherRegistryKeys.UpdateSearchMode, out var value, ApplicationType.Stable);
                return value;
            }
            set
            {
                LauncherRegistryHelper.WriteValue(LauncherRegistryKeys.UpdateSearchMode, value);
                OnPropertyChanged();
            }
        }

        public ApplicationType? CurrentUpdateSearchOption
        {
            get
            {
                LauncherRegistryHelper.GetValue<ApplicationType?>(LauncherRegistryKeys.SessionUpdateSearchMode, out var value);
                return value;
            }
            set
            {
                if (value is null)
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
                return current != ApplicationType.Stable;
            }
        }

        public FallbackSuppression UpdateFallbackSuppression
        {
            get
            {
                LauncherRegistryHelper.GetValueOrDefault(LauncherRegistryKeys.SuppressFallbackUpdate, out var value, FallbackSuppression.Always);
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
                if (CurrentUpdateSearchOption != null)
                    return UpdateMode.Explicit;
                switch (UpdateFallbackSuppression)
                {
                    case FallbackSuppression.Always:
                        return UpdateMode.NoFallback;
                    case FallbackSuppression.Ask:
                        return UpdateMode.AskFallbackStable;
                    default:
                        return UpdateMode.FallbackStable;
                }
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

        public static FocLauncherInformation Instance => _instance ??= new FocLauncherInformation();

        private FocLauncherInformation()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}