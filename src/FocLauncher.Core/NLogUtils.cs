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
#if DEBUG
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);
#else
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
#endif
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
