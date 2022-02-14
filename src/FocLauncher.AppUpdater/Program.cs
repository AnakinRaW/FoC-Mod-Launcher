using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using CommandLine;
using FocLauncher.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using Sklavenwalker.CommonUtilities.FileSystem;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
#if DEBUG
using NLog.Conditions;
#endif

namespace FocLauncher.AppUpdater;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var exitCode = (int) ExternalUpdaterResult.NoUpdate;
        var parserResult = Parser.Default.ParseArguments<LauncherUpdaterOptions>(args);
        await parserResult.WithParsedAsync(async options =>
        {
            var services = CreateServices(options);
            var logger = services.GetService<ILoggerFactory>()?.CreateLogger(typeof(Program));
            try
            {
                logger?.LogTrace("Application Updater");
                logger?.LogTrace($"Raw Command line: {string.Join(",", args)}");
                var application = new Application(options, services);
                exitCode = (int) await application.Run();
                logger?.LogTrace($"Exit Code: {exitCode}");
            }
            catch (Exception e)
            {
                logger?.LogCritical(e, e.Message);
#if DEBUG
                Console.WriteLine("Press enter to close!");
                Console.ReadKey();
                exitCode = (int)ExternalUpdaterResult.UpdateFailedWithRestore;
#endif
            }
        });

        return exitCode;
    }

    private static IServiceProvider CreateServices(LauncherUpdaterOptions options)
    {
        var services = new ServiceCollection();
        var fs = new FileSystem();
        services.AddSingleton<IFileSystem>(fs);
        services.AddSingleton<IFileSystemService>(new FileSystemService(fs));

        services.AddLogging(b =>
        {
            b.ClearProviders();
#if DEBUG
            b.SetMinimumLevel(LogLevel.Trace);
#else
            b.SetMinimumLevel(LogLevel.Information);
#endif
            b.AddNLog(CreateLogging(options.LogFile));
        });

        return services.BuildServiceProvider();
    }

    private static LoggingConfiguration CreateLogging(string? logfile)
    {
        var config = new LoggingConfiguration();

        Target? logfileTarget = null;
        if (!string.IsNullOrEmpty(logfile) && File.Exists(logfile))
            logfileTarget = new FileTarget("logfile") { FileName = logfile };
#if DEBUG
        if (logfileTarget != null)
            config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, logfileTarget);
        var consoleTarget = new ColoredConsoleTarget();
        var highlightRule = new ConsoleRowHighlightingRule
        {
            Condition = ConditionParser.ParseExpression("level == LogLevel.Info"),
            ForegroundColor = ConsoleOutputColor.Green
        };
        consoleTarget.RowHighlightingRules.Add(highlightRule);

        config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, consoleTarget);
#else
        if (logfileTarget != null)
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logfileTarget);
#endif
        return config;
    }
}