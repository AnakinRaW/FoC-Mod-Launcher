using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

namespace AnakinRaW.ApplicationManifestCreator;

internal class ManifestCreator
{
    public ManifestCreator(ManifestCreatorOptions options, IServiceProvider serviceProvider)
    {
        
    }

    public async Task<int> Run()
    {
        return 0;
    }
}

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        Console.WriteLine($"Raw Command line: {Environment.CommandLine}");
        return await Parser.Default.ParseArguments<ManifestCreatorOptions>(args)
            .MapResult(CreateManifest, ErrorArgs);
    }

    private static Task<int> ErrorArgs(IEnumerable<Error> arg)
    {
        return Task.FromResult(0xA0);
    }

    private static async Task<int> CreateManifest(ManifestCreatorOptions opts)
    {
        var services = CreateServices();
        var logger = services.GetService<ILoggerFactory>()?.CreateLogger(typeof(Program));
        try
        {
            var result = await new ManifestCreator(opts, services).Run();
            logger?.LogTrace($"ManifestCreator finished with result: {result}");
            return 0;
        }
        catch (Exception e)
        {
            logger?.LogCritical(e, e.Message);
            return e.HResult;
        }
    }

    private static IServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        var fileSystem = new FileSystem();
        services.AddSingleton<IFileSystem>(fileSystem);

        services.AddLogging(l =>
        {
            l.ClearProviders();
#if DEBUG
            l.AddConsole().SetMinimumLevel(LogLevel.Trace);
            l.AddDebug().SetMinimumLevel(LogLevel.Trace);
#endif
        }).Configure<LoggerFilterOptions>(o =>
        {
#if DEBUG
            o.AddFilter<DebugLoggerProvider>(null, LogLevel.Trace);
#endif
        });

        return services.BuildServiceProvider();
    }
}

internal class ManifestCreatorOptions
{
    [Option('a', "applicationFile", Required = true, HelpText = "The file of the application to create the manifest for.")]
    public string ApplicationFile { get; init; }

    [Option('c', "configFile", Required = true, HelpText = "File to configure the tool.")]
    public string Configuration { get; init; }

    [Option("appDataFiles", HelpText = "Files which shall be installed to the application's AppData directory")]
    public ICollection<string> AppDataComponents { get; init; }

    [Option("installDirFiles", HelpText = "Files which shall be installed to the application's install directory")]
    public ICollection<string> InstallDirComponents { get; init; }
}

