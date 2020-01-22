using System;

namespace FocLauncher
{
    public static class LauncherConstants
    {
        public const string ApplicationBaseVariable = "APPLICATIONBASE";

        private static readonly Lazy<string> ApplicationBasePathLazy = new Lazy<string>(() => Environment.GetEnvironmentVariable(ApplicationBaseVariable));

        public static string ApplicationBasePath => ApplicationBasePathLazy.Value;
    }
}
