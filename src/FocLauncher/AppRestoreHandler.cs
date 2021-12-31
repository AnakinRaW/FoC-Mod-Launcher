using System;
using System.Windows.Input;
using FocLauncher.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sklavenwalker.CommonUtilities.FileSystem;
using Validation;

namespace FocLauncher;

internal class AppRestoreHandler
{
    private readonly ILogger? _logger;
    private readonly ILauncherRegistry _registry;
    private readonly IFileSystemService _fileSystemService;
    private readonly ILauncherEnvironment _environment;

    public AppRestoreHandler(IServiceProvider services)
    {
        Requires.NotNull(services, nameof(services));
        _logger = services.GetService<ILogger>();
        _registry = services.GetRequiredService<ILauncherRegistry>();
        _fileSystemService = services.GetRequiredService<IFileSystemService>();
        _environment = services.GetRequiredService<ILauncherEnvironment>();
    }

    public void RestoreIfRequested()
    {
        if (_registry.Restore || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            RestoreApplication();
    }

    public void RestoreApplication()
    {
        try
        {
            _fileSystemService.DeleteDirectoryWithRetry(_environment.ApplicationLocalDirectory);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Unable to delete local app directory, because: {e.Message}");
            throw;
        }
        _registry.Reset();
        _environment.ApplicationLocalDirectory.Create();
        _logger?.LogTrace("Test");
    }
}