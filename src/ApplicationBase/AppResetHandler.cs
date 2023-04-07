﻿using System;
using System.IO;
using System.Security.AccessControl;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater.Registry;
using AnakinRaW.CommonUtilities.FileSystem;
using AnakinRaW.CommonUtilities.FileSystem.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace AnakinRaW.ApplicationBase;

public class AppResetHandler
{
    private readonly IApplicationUpdaterRegistry _registry;
    private readonly IFileSystemService _fileSystemService;
    private readonly IApplicationEnvironment _environment;
    private readonly IWindowsPathService _pathService;
    private readonly ILogger? _logger;

    public AppResetHandler(IServiceProvider services)
    {
        Requires.NotNull(services, nameof(services));
        _registry = services.GetRequiredService<IApplicationUpdaterRegistry>();
        _fileSystemService = services.GetRequiredService<IFileSystemService>();
        _environment = services.GetRequiredService<IApplicationEnvironment>();
        _pathService = services.GetRequiredService<IWindowsPathService>();
        _logger = services.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public void ResetIfNecessary()
    {
        if (!_environment.ApplicationLocalDirectory.Exists || _registry.Reset)
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
        }
        catch (Exception e)
        {
            _logger?.LogCritical(e, e.Message);
            throw;
        }
    }
}