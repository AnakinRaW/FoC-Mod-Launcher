using System;

namespace FocLauncher
{
    public static class LauncherConstants
    {
        public const string ApplicationBaseVariable = "APPLICATIONBASE";

        // TODO: Get from Assembly
        public const string ProductName = "FoC-Launcher";
        public const string Author = "AnakinSklavenwalker";

        private static readonly Lazy<string> ApplicationBasePathLazy = new Lazy<string>(() => Environment.GetEnvironmentVariable(ApplicationBaseVariable));

        public static string ApplicationBasePath => ApplicationBasePathLazy.Value;
    }
}
