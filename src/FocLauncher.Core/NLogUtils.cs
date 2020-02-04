using System;
using System.IO;
using NLog;
using LogLevel = NLog.LogLevel;

namespace FocLauncher
{
    public static class NLogUtils
    {
        public static void SetLoggingForAppDomain()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logFilePath = Path.Combine(LauncherConstants.ApplicationBasePath, "log.txt");
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = logFilePath };
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);
            LogManager.Configuration = config;
        }

        public static void DeleteOldLogFile()
        {
            var logFilePath = Path.Combine(LauncherConstants.ApplicationBasePath, "log.txt");
            try
            {
                if (File.Exists(logFilePath))
                    File.Delete(logFilePath);
            }
            catch (IOException )
            {
            }
        }
    }
}
