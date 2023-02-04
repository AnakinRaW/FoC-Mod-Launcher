using System;
using System.IO;
using System.Security.AccessControl;
using System.Windows.Input;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using FocLauncher.Services;
using FocLauncher.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using AnakinRaW.CommonUtilities.FileSystem;
using AnakinRaW.CommonUtilities.FileSystem.Windows;
using Validation;

namespace FocLauncher;

internal class AppRestoreHandler
{
    private readonly ILauncherRegistry _registry;
    private readonly IFileSystemService _fileSystemService;
    private readonly ILauncherEnvironment _environment;
    private readonly IWindowsPathService _pathService;

    public AppRestoreHandler(IServiceProvider services)
    {
        Requires.NotNull(services, nameof(services));
        _registry = services.GetRequiredService<ILauncherRegistry>();
        _fileSystemService = services.GetRequiredService<IFileSystemService>();
        _environment = services.GetRequiredService<ILauncherEnvironment>();
        _pathService = services.GetRequiredService<IWindowsPathService>();
    }

    public void RestoreIfNecessary()
    {
        if (!_environment.ApplicationLocalDirectory.Exists || _registry.Restore || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            RestoreApplication();
    }

    public void RestoreApplication()
    {
        try
        {
            var appLocalPath = _environment.ApplicationLocalDirectory;
            if (!_pathService.UserHasDirectoryAccessRights(appLocalPath.Parent.FullName, FileSystemRights.CreateDirectories))
                throw new IOException($"Permission on '{appLocalPath}' denied: Creating a new directory");

            _fileSystemService.DeleteDirectoryWithRetry(_environment.ApplicationLocalDirectory);
            _registry.Reset();
            _environment.ApplicationLocalDirectory.Create();
        }
        catch (Exception e)
        {
            new UnhandledExceptionDialog(new UnhandledExceptionDialogViewModel(e)).ShowModal();
            throw;
        }
    }
}