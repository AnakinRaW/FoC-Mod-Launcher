using FocLauncher.Shared;

namespace FocLauncherHost
{ 
    internal class LauncherStartOptions
    {
        public bool SkipUpdate { get; set; }

        public ExternalUpdaterResult LastUpdaterResult { get; set; }
    }
}