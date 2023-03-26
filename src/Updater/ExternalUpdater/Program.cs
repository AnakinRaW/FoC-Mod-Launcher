using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities.FileSystem;
using AnakinRaW.ExternalUpdater.CLI.Arguments;
using AnakinRaW.ExternalUpdater.Tools;
using AnakinRaW.ExternalUpdater.Utilities;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
#if DEBUG
using Microsoft.Extensions.Logging.Debug;
#endif

namespace AnakinRaW.ExternalUpdater;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
#if DEBUG
        Console.WriteLine($"Raw Command line: {Environment.CommandLine}");
#endif

        return await Parser.Default.ParseArguments<RestartArguments, UpdateArguments>(args)
            .MapResult(
                (RestartArguments opts) => ExecuteApplication(opts),
                (UpdateArguments opts) => ExecuteApplication(opts),
                ErrorArgs);
    }

    private static async Task<int> ExecuteApplication(ExternalUpdaterArguments args)
    {
        var services = CreateServices(args);
        var logger = services.GetService<ILoggerFactory>()?.CreateLogger(typeof(Program));
        try
        {
            var tool = new ToolFactory().Create(args, services);
            var result = await tool.Run();
            logger?.LogTrace($"Tool '{tool}' finished with result: {result}");
            return 0;
        }
        catch (Exception e)
        {
            logger?.LogCritical(e, e.Message);
            return e.HResult;
        }
        finally
        {
#if DEBUG
            Console.WriteLine("Press enter to close!");
            Console.ReadKey();
#endif
        }
    }

    private static Task<int> ErrorArgs(IEnumerable<Error> arg)
    {
        return Task.FromResult(0xA0);
    }

    private static IServiceProvider CreateServices(ExternalUpdaterArguments arguments)
    {
        var services = new ServiceCollection();
        var fileSystem = new FileSystem();
        services.AddSingleton<IFileSystem>(fileSystem);
        services.AddSingleton<IFileSystemService>(new FileSystemService(fileSystem));
        services.AddSingleton<IProcessTools>(sp => new ProcessTools(sp));

        services.AddLogging(l =>
        {
            l.ClearProviders();
#if DEBUG
            l.AddConsole().SetMinimumLevel(LogLevel.Trace);
            l.AddDebug().SetMinimumLevel(LogLevel.Trace);
#endif
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
            var logPath = arguments.LogFile ?? "log.txt";
            var fileLogLevel = LogLevel.Information;
#if DEBUG
            fileLogLevel = LogLevel.Trace;
#endif
            builder.AddFile("log.txt", fileLogLevel);
        }
    }
}