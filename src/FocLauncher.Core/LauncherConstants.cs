using System;
using System.IO;

namespace FocLauncher
{
    public static class LauncherConstants
    {
        public const string ApplicationBaseVariable = "APPLICATIONBASE";
        public const string ExecutablePathVariable = "EXECUTABLE";

        public const string UpdaterFileName = "FocLauncher.AppUpdater.exe";
        public const string LogFileName = "FocLauncherLog.txt";

        public const string ProductName = "FoC-Launcher";
        public const string Author = "AnakinSklavenwalker";

        private static readonly Lazy<string> ApplicationBasePathLazy = new Lazy<string>(() => Environment.GetEnvironmentVariable(ApplicationBaseVariable));
        private static readonly Lazy<string> ExecutablePathLazy = new Lazy<string>(() => Environment.GetEnvironmentVariable(ExecutablePathVariable));

        public static string ApplicationBasePath => ApplicationBasePathLazy.Value;
        public static string ExecutablePath => ExecutablePathLazy.Value;

        public static string UpdaterPath => Path.Combine(ApplicationBasePath, UpdaterFileName);
        public static string LogFilePath => Path.Combine(ApplicationBasePath, LogFileName);
    }
}
