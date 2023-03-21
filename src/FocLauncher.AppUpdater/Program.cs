using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using CommandLine;
using FocLauncher.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AnakinRaW.CommonUtilities.FileSystem;
using Microsoft.Extensions.Logging.Debug;
using Serilog.Extensions.Logging;

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
        var fileSystem = new FileSystem();
        services.AddSingleton<IFileSystem>(fileSystem);
        services.AddSingleton<IFileSystemService>(new FileSystemService(fileSystem));

        services.AddLogging(l =>
        {
#if DEBUG
            l.AddDebug();
#endif
            l.AddConsole();
            SetFileLogging(l);
        }).Configure<LoggerFilterOptions>(o =>
        {
#if DEBUG
            o.AddFilter<DebugLoggerProvider>(null, LogLevel.Trace);
#endif
            o.AddFilter<SerilogLoggerProvider>(null, LogLevel.Trace);
        });

        return services.BuildServiceProvider();


        void SetFileLogging(ILoggingBuilder builder)
        {
            var logPath = options.LogFile ?? "log.txt";
            var fileLogLevel = LogLevel.Information;
#if DEBUG
            fileLogLevel = LogLevel.Trace;
#endif
            builder.AddFile(logPath, fileLogLevel);
        }
    }
}