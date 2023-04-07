using System;
using System.IO.Abstractions;
using System.Reflection;
using AnakinRaW.ApplicationBase;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.StatusBar;
using FocLauncher.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Serilog.Extensions.Logging;
using AnakinRaW.CommonUtilities.DownloadManager.Verification.HashVerification;
using AnakinRaW.CommonUtilities.DownloadManager.Verification;
using AnakinRaW.CommonUtilities.DownloadManager;
using AnakinRaW.CommonUtilities.DownloadManager.Configuration;
using AnakinRaW.CommonUtilities.Windows;
using FocLauncher.Imaging;

namespace FocLauncher;

internal class Program : ProgramBase
{
    [STAThread]
    private static int Main(string[] args)
    {
        return new Program().Run(args);
    }

    protected override IApplicationEnvironment CreateEnvironment(IServiceProvider serviceProvider)
    {
        var fs = serviceProvider.GetRequiredService<IFileSystem>();
        return new LauncherEnvironment(Assembly.GetExecutingAssembly(), fs);
    }

    protected override void CreateCoreServicesAfterEnvironment(IServiceCollection serviceCollection)
    {
        using var services = serviceCollection.BuildServiceProvider();
        var fileSystem = services.GetRequiredService<IFileSystem>();
        var environment = services.GetRequiredService<IApplicationEnvironment>();
        SetLogging(serviceCollection, fileSystem, environment);
    }

    protected override int ExecuteInternal(string[] args, IServiceCollection serviceCollection)
    {
        BuildApplicationServices(serviceCollection);
        return new LauncherApplication(serviceCollection.BuildServiceProvider()).Run();
    }

    private void BuildApplicationServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ILauncherRegistry>(sp => new LauncherRegistry(sp));

        serviceCollection.AddSingleton<IDownloadManager>(sp => new DownloadManager(sp));
        serviceCollection.AddSingleton<IVerificationManager>(sp =>
        {
            var vm = new VerificationManager(sp);
            vm.RegisterVerifier("*", new HashVerifier(sp));
            return vm;
        });

        serviceCollection.AddSingleton(CreateDownloadConfiguration());

        serviceCollection.AddTransient<IStatusBarFactory>(_ => new LauncherStatusBarFactory()); 
        serviceCollection.AddTransient(_ => ConnectionManager.Instance);

        serviceCollection.AddApplicationFramework();
        serviceCollection.AddApplicationBaseWpf(ImageKeys.AppIcon);
    }

    private static IDownloadManagerConfiguration CreateDownloadConfiguration()
    {
        return new DownloadManagerConfiguration { VerificationPolicy = VerificationPolicy.Optional };
    }

    private static void SetLogging(IServiceCollection serviceCollection, IFileSystem fileSystem, IApplicationEnvironment environment)
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
            var version = environment.AssemblyInfo.InformationalAsSemVer();
            if (version is not null && version.IsPrerelease)
                fileLogLevel = LogLevel.Debug;
#if DEBUG
            logPath = "log.txt";
            fileLogLevel = LogLevel.Trace;
#endif
            builder.AddFile(logPath, fileLogLevel);
        }
    }
}