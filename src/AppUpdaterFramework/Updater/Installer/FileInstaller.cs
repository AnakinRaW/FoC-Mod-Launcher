using System;
using System.IO.Abstractions;
using System.Threading;
using AnakinRaW.AppUpaterFramework.Metadata.Component;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.AppUpaterFramework.Metadata.Update;
using AnakinRaW.AppUpaterFramework.Updater.Progress;
using AnakinRaW.AppUpaterFramework.Updater.Tasks;
using AnakinRaW.CommonUtilities.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace AnakinRaW.AppUpaterFramework.Updater.Installer;

internal class FileInstaller : IInstaller
{
    public event EventHandler<ProgressEventArgs>? Progress;

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger? _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IFileSystemService _fileSystemHelper;

    public FileInstaller(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _fileSystemHelper = serviceProvider.GetRequiredService<IFileSystemService>();
    }

    public InstallResult Install(IInstallableComponent component, string source, ProductVariables variables, CancellationToken token = default)
    {
        if (component is not SingleFileComponent singleFileComponent)
            throw new NotSupportedException($"component must be of type {nameof(SingleFileComponent)}");

        try
        {
            OnProgress(new ProgressEventArgs(component.GetDisplayName(), 0.0, ProgressType.Install));
        }
        finally
        {
            OnProgress(new ProgressEventArgs(component.GetDisplayName(), 1.0, ProgressType.Install));
        }

        return InstallResult.Success;
    }

    public InstallResult Remove(IInstallableComponent component, ProductVariables variables, CancellationToken token = default)
    {
        if (component is not SingleFileComponent singleFileComponent)
            throw new NotSupportedException($"component must be of type {nameof(SingleFileComponent)}");
        try
        {
            OnProgress(new ProgressEventArgs(component.GetDisplayName(), 0.0, ProgressType.Install));
            try
            {
                _logger?.LogInformation($"Started: removing {component.GetDisplayName()}");
                return DoAction(() => RemoveCore(singleFileComponent, variables));
            }
            finally
            {
                _logger?.LogInformation($"Completed: removing {component.GetDisplayName()}");
            }
        }
        catch (OperationCanceledException)
        {
            _logger?.LogInformation("User canceled during component remove.");
            return InstallResult.Cancel;
        }
        catch (Exception e)
        {
            LogFailure(component, UpdateAction.Delete, e.ToString());
            return InstallResult.FailureException;
        }
        finally
        {
            OnProgress(new ProgressEventArgs(component.GetDisplayName(), 1.0, ProgressType.Install));
        }
    }

    private InstallResult RemoveCore(SingleFileComponent component, ProductVariables variables)
    {
        var filePath = component.GetFilePath(_fileSystem, variables);

        var restartPending = false;
        var deleteResult = DeleteFile(filePath, out var restartRequired);
        
        restartPending |= restartRequired;
        
        if (!deleteResult && !restartRequired)
            return InstallResult.Failure;

        return restartPending ? InstallResult.SuccessRestartRequired : InstallResult.Success;
    }

    private bool DeleteFile(IFileInfo file, out bool restartRequired)
    {
        restartRequired = false;

        if (!file.Exists)
        {
            _logger?.LogTrace($"'{file}' file is already deleted.");
            return true;
        }

        var deleteSuccess = _fileSystemHelper.DeleteFileWithRetry(file, 2, 500, (ex, _) =>
        {
            _logger?.LogTrace(
                $"Error occurred while deleting file '{file}'. Error details: {ex.Message}. Retrying after {0.5f} seconds...");
            return true;
        });
        
        if (deleteSuccess)
            _logger?.LogInformation($"{file} file deleted.");
        else
        {
            // TODO: _lockedFiles.Add(file);
            _logger?.LogInformation($"{file} file is scheduled for deletion after restarting.");
        }

        return deleteSuccess;
    }

    private void LogFailure(IProductComponent? component, UpdateAction executeAction, string details)
    {
        _logger?.LogError(component != null
            ? $"Component '{component.GetDisplayName()}' failed to {executeAction.ToString().ToLowerInvariant()}. {details}"
            : $"Failed to {executeAction.ToString().ToLowerInvariant()}. {details}");
    }

    private InstallResult DoAction(Func<InstallResult> action)
    {
        var result = action();
        // TODO Locked files
        return result;
    }

    protected virtual void OnProgress(ProgressEventArgs e)
    {
        Progress?.Invoke(this, e);
    }
}