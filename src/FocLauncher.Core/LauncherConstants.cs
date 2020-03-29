using System;
using System.IO;
using FocLauncher.Utilities;

namespace FocLauncher
{
    public static class LauncherConstants
    {
        public const string UpdateMetadataServerUrl = "https://raw.githubusercontent.com/AnakinSklavenwalker/FoC-Mod-Launcher/master";

        public const string ApplicationBaseVariable = "APPLICATIONBASE";
        public const string ExecutablePathVariable = "EXECUTABLE";

        public const string UpdaterFileName = "FocLauncher.AppUpdater.exe";
        public const string LogFileName = "FocLauncherLog.txt";
        public const string UpdateMetadataFileName = "LauncherUpdateData.xml";
        public const string LauncherFileName = "FoC Launcher.exe";

        public const string ProductName = "FoC-Launcher";
        public const string Author = "AnakinSklavenwalker";

        private static readonly Lazy<string> ApplicationBasePathLazy = new Lazy<string>(() => Environment.GetEnvironmentVariable(ApplicationBaseVariable));
        private static readonly Lazy<string> ExecutablePathLazy = new Lazy<string>(() => Environment.GetEnvironmentVariable(ExecutablePathVariable));

        public static string ApplicationBasePath => ApplicationBasePathLazy.Value;
        public static string ExecutablePath => ExecutablePathLazy.Value;

        public static string UpdaterPath => Path.Combine(ApplicationBasePath, UpdaterFileName);
        public static string LogFilePath => Path.Combine(ApplicationBasePath, LogFileName);
        public static string UpdateMetadataPath => UrlCombine.Combine(UpdateMetadataServerUrl, UpdateMetadataFileName);
    }
}
