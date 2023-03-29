using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using AnakinRaW.ExternalUpdater.Options;
using AnakinRaW.ExternalUpdater.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnakinRaW.ExternalUpdater.Tools;

internal abstract class ToolBase<T> : ITool where T : ExternalUpdaterOptions
{
    public T Options { get; }

    protected IServiceProvider ServiceProvider { get; }

    protected ILogger? Logger { get; }

    protected IProcessTools ProcessTools { get; }

    protected IFileSystem FileSystem { get; }

    protected ToolBase(T options, IServiceProvider serviceProvider)
    {
        Options = options;
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        ProcessTools = serviceProvider.GetRequiredService<IProcessTools>();
        FileSystem = serviceProvider.GetRequiredService<IFileSystem>();
    }

    public abstract Task<ExternalUpdaterResult> Run();

    public override string ToString()
    {
        return GetType().Name;
    }
}