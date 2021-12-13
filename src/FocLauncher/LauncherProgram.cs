using System;
using System.IO.Abstractions;
using FocLauncher.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Sklavenwalker.CommonUtilities.Wpf.Theming;
using Sklavenwalker.Wpf.CommandBar;

namespace FocLauncher;

internal static class LauncherProgram
{
    [STAThread]
    private static int Main()
    {
        var launcherServices = BuildLauncherServiceProvider();

        int exitCode;
        using (GetUnhandledExceptionHandler(launcherServices)) 
            exitCode = Execute(launcherServices);
        return exitCode;
    }

    internal static int Execute(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(LauncherProgram));
        logger.LogTrace($"FoC Mod Launcher Version: {LauncherAssemblyInfo.InformationalVersion}");
        logger.LogTrace($"Raw Command line: {Environment.CommandLine}");
        // TODO: Log parsed CMD
        var exitCode = ExecuteInternal(services, logger);
        logger.LogTrace($"Exit Code: {exitCode}");
        return exitCode;
    }

    private static int ExecuteInternal(IServiceProvider services, ILogger logger)
    {
        try
        {
            var exitCode = new LauncherApplication(services).Run();
            logger.LogTrace($"Closing the installer with exit code {exitCode}");
            return exitCode;
        }
        catch (Exception ex)
        {
            var str = "Launcher failed with an uncaught exception: " + ex.Message;
            logger.LogError(ex, str);
            return ex.HResult;
        }
    }

    private static UnhandledExceptionHandler GetUnhandledExceptionHandler(IServiceProvider launcherServices)
    {
        // TODO
        return new UnhandledExceptionHandler();
    }

    private static IServiceProvider BuildLauncherServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        CreateProgramServices(serviceCollection);
        serviceCollection.AddSingleton<IThemeManager>(new LauncherThemeManager());
        serviceCollection.AddTransient<IStatusBarFactory>(_ => new LauncherStatusBarFactory()); 
        
        return serviceCollection.BuildServiceProvider();
    }
    

    private static void CreateProgramServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IFileSystem>(new FileSystem());

        // TODO: File Logging
        serviceCollection.AddLogging(l =>
        {
#if DEBUG
            l.AddDebug();
#endif
            l.AddConsole();
        }).Configure<LoggerFilterOptions>(o =>
        {
#if DEBUG
            o.AddFilter<DebugLoggerProvider>(null, LogLevel.Trace);
#endif
            o.AddFilter<ConsoleLoggerProvider>(null, LogLevel.Warning);
        });
    }
}