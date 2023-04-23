using System;
using System.IO.Abstractions;
using System.Reflection;
using AnakinRaW.ApplicationBase;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.StatusBar;
using FocLauncher.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Serilog.Extensions.Logging;
using AnakinRaW.CommonUtilities.Registry;
using AnakinRaW.CommonUtilities.Registry.Windows;
using AnakinRaW.CommonUtilities.Windows;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using FocLauncher.Imaging;

namespace FocLauncher;


internal class LauncherBootstrapper : WpfBootstrapper
{
    protected override ImageKey AppIcon => ImageKeys.AppIcon;

    [STAThread]
    private static int Main(string[] args)
    {
        return new LauncherBootstrapper().Run(args);
    }

    protected override int Execute(string[] args, IServiceCollection serviceCollection)
    {
        BuildApplicationServices(serviceCollection);
        return new LauncherApplication(serviceCollection.BuildServiceProvider()).Run();
    }

    protected override IApplicationEnvironment CreateEnvironment(IServiceProvider serviceProvider)
    {
        return new LauncherEnvironment(Assembly.GetExecutingAssembly(), serviceProvider);
    }

    protected override IRegistry CreateRegistry()
    {
        return new WindowsRegistry();
    }

    protected override void SetupLogging(IServiceCollection serviceCollection, IFileSystem fileSystem,
        IApplicationEnvironment applicationEnvironment)
    {
        serviceCollection.AddLogging(l =>
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


        void SetFileLogging(ILoggingBuilder builder)
        {
            var logPath = fileSystem.Path.Combine(
                fileSystem.Path.GetTempPath(), LauncherEnvironment.LauncherLogDirectoryName, "launcher.log");
            var fileLogLevel = LogLevel.Information;
            var version = applicationEnvironment.AssemblyInfo.InformationalAsSemVer();
            if (version is not null && version.IsPrerelease)
                fileLogLevel = LogLevel.Debug;
#if DEBUG
            logPath = "log.txt";
            fileLogLevel = LogLevel.Trace;
#endif
            builder.AddFile(logPath, fileLogLevel);
        }
    }

    private static void BuildApplicationServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ILauncherRegistry>(sp => new LauncherRegistry(sp));
        serviceCollection.AddTransient<IStatusBarFactory>(_ => new LauncherStatusBarFactory()); 
        serviceCollection.AddTransient(_ => ConnectionManager.Instance);
    }
}