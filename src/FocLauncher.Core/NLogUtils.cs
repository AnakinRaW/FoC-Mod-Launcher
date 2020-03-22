using System.IO;
using NLog;
using NLog.Layouts;
using LogLevel = NLog.LogLevel;

namespace FocLauncher
{
    public static class NLogUtils
    {
        public static void SetLoggingForAppDomain()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var logFilePath = LauncherConstants.LogFilePath;
            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = logFilePath,
                Layout = new LayoutWithHeaderAndFooter
                {
                    Header = new SimpleLayout("/**************** Foc Launcher by Anakin Sklavenwalker ****************/"),
                    Layout = new SimpleLayout("${longdate}|${level:uppercase=true}|${logger}|${message}")
                }
            };
#if DEBUG
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);
#else
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
#endif
            LogManager.Configuration = config;
        }

        public static void DeleteOldLogFile()
        {
            var logFilePath = LauncherConstants.LogFilePath;
            try
            {
                if (File.Exists(logFilePath))
                    File.Delete(logFilePath);
            }
            catch (IOException)
            {
            }
        }
    }
}
