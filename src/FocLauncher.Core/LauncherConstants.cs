using System;

namespace FocLauncher
{
    public static class LauncherConstants
    {
        public const string ApplicationBaseVariable = "APPLICATIONBASE";
        public const string ExecutablePathVariable = "%EXECUTABLE%";

        // TODO: Get from Assembly
        public const string ProductName = "FoC-Launcher";
        public const string Author = "AnakinSklavenwalker";

        private static readonly Lazy<string> ApplicationBasePathLazy = new Lazy<string>(() => Environment.GetEnvironmentVariable(ApplicationBaseVariable));
        private static readonly Lazy<string> ExecutablePathLazy = new Lazy<string>(() => Environment.GetEnvironmentVariable(ExecutablePathVariable));

        public static string ApplicationBasePath => ApplicationBasePathLazy.Value;
        public static string ExecutablePath => ExecutablePathLazy.Value;
    }
}
