using System;
using System.IO;
using System.Security.AccessControl;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater.Registry;
using AnakinRaW.CommonUtilities.FileSystem;
using AnakinRaW.CommonUtilities.FileSystem.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace AnakinRaW.ApplicationBase.Services;

internal class AppResetHandler : IAppResetHandler
{
    private readonly IApplicationUpdaterRegistry _registry;
    private readonly IFileSystemService _fileSystemService;
    private readonly IApplicationEnvironment _environment;
    private readonly IWindowsPathService _pathService;
    private readonly ILogger? _logger;

    public IServiceProvider Services { get; }

    public AppResetHandler(IServiceProvider services)
    {
        Requires.NotNull(services, nameof(services));
        Services = services;
        _registry = services.GetRequiredService<IApplicationUpdaterRegistry>();
        _fileSystemService = services.GetRequiredService<IFileSystemService>();
        _environment = services.GetRequiredService<IApplicationEnvironment>();
        _pathService = services.GetRequiredService<IWindowsPathService>();
        _logger = services.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public void ResetIfNecessary()
    {
        if (RequiresReset())
            ResetApplication();
    }

    public void ResetApplication()
    {
        try
        {
            var appLocalPath = _environment.ApplicationLocalDirectory;
            if (!_pathService.UserHasDirectoryAccessRights(appLocalPath.Parent!.FullName, FileSystemRights.CreateDirectories))
                throw new IOException($"Permission on '{appLocalPath}' denied: Creating a new directory");

            _fileSystemService.DeleteDirectoryWithRetry(_environment.ApplicationLocalDirectory);
            _registry.Clear();
            _environment.ApplicationLocalDirectory.Create();

            OnReset();
        }
        catch (Exception e)
        {
            _logger?.LogCritical(e, e.Message);
            OnResetFailed(e);
            throw;
        }
    }

    protected virtual bool RequiresReset()
    {
        return !_environment.ApplicationLocalDirectory.Exists || _registry.Reset;
    }

    protected virtual void OnReset()
    {
    }

    protected virtual void OnResetFailed(Exception exception)
    {
    }
}