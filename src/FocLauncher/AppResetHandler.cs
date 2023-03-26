using System;
using System.IO;
using System.Security.AccessControl;
using System.Windows.Input;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using Microsoft.Extensions.DependencyInjection;
using AnakinRaW.CommonUtilities.FileSystem;
using AnakinRaW.CommonUtilities.FileSystem.Windows;
using FocLauncher.ViewModels.Dialogs;
using Validation;
using AnakinRaW.AppUpdaterFramework.ExternalUpdater.Registry;

namespace FocLauncher;

internal class AppResetHandler
{
    private readonly IApplicationUpdaterRegistry _registry;
    private readonly IFileSystemService _fileSystemService;
    private readonly ILauncherEnvironment _environment;
    private readonly IWindowsPathService _pathService;

    public AppResetHandler(IServiceProvider services)
    {
        Requires.NotNull(services, nameof(services));
        _registry = services.GetRequiredService<IApplicationUpdaterRegistry>();
        _fileSystemService = services.GetRequiredService<IFileSystemService>();
        _environment = services.GetRequiredService<ILauncherEnvironment>();
        _pathService = services.GetRequiredService<IWindowsPathService>();
    }

    public void ResetIfNecessary()
    {
        if (!_environment.ApplicationLocalDirectory.Exists || _registry.Reset || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
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
            new UnhandledExceptionDialog(new UnhandledExceptionDialogViewModel(e)).ShowModal();
            throw;
        }
    }
}