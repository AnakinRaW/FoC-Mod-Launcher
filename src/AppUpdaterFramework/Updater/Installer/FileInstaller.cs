﻿using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using AnakinRaW.AppUpaterFramework.Interaction;
using AnakinRaW.AppUpaterFramework.Metadata.Component;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.AppUpaterFramework.Updater.Tasks;
using AnakinRaW.CommonUtilities.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;
using Vanara.PInvoke;

namespace AnakinRaW.AppUpaterFramework.Updater.Installer;

internal class FileInstaller : InstallerBase
{
    private readonly ILogger? _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IFileSystemService _fileSystemHelper;
    private readonly IInteractionHandler _interactionHandler;

    public FileInstaller(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _fileSystemHelper = serviceProvider.GetRequiredService<IFileSystemService>();
        //_interactionHandler = serviceProvider.GetRequiredService<IInteractionHandler>();
    }
    
    protected override InstallResult RemoveCore(IInstallableComponent component, ProductVariables variables, CancellationToken token)
    {
        if (component is not SingleFileComponent singleFileComponent)
            throw new NotSupportedException($"Component must be of type {nameof(SingleFileComponent)}");

        var filePath = singleFileComponent.GetFile(_fileSystem, variables);
        return ExecuteWithInteractiveRetry(component, 
            () => DeleteFile(filePath),
            interaction => HandleLockedFile(component, filePath, interaction), 
            token);
    }

    private InstallerInteractionResult HandleLockedFile(IInstallableComponent component, IFileInfo file, InstallOperationResult operationResult)
    {
        if (operationResult != InstallOperationResult.LockedFile)
            throw new NotSupportedException($"OperationResult '{operationResult}' is not supported by this installer");

        return new InstallerInteractionResult(InstallResult.Failure, true);
    }

    protected override InstallResult InstallCore(IInstallableComponent component, string source, ProductVariables variables, CancellationToken token)
    {
        Requires.NotNull(source, nameof(source));
        return InstallResult.Success;
    }

    private InstallOperationResult DeleteFile(IFileInfo file)
    {
        if (!file.Exists)
        {
            _logger?.LogTrace($"'{file}' file is already deleted.");
            return InstallOperationResult.Success;
        }
        try
        {
            var deleteSuccess = _fileSystemHelper.DeleteFileWithRetry(file, 2, 500, (ex, _) =>
            {
                _logger?.LogTrace(
                    $"Error occurred while deleting file '{file}'. Error details: {ex.Message}. Retrying after {0.5f} seconds...");
                return true;
            });
            _logger?.LogTrace(deleteSuccess ? $"File '{file}' deleted." : $"File '{file}' was not deleted");
            return deleteSuccess ? InstallOperationResult.Success : InstallOperationResult.Failed;
        }
        catch (IOException e) when(new HRESULT(e.HResult).Code == Win32Error.ERROR_SHARING_VIOLATION)
        {
            _logger?.LogWarning($"File '{file}' is locked");
            return InstallOperationResult.LockedFile;
        }
        catch(Exception e)
        {
            _logger?.LogError(e, $"File '{file}' could not be deleted: {e.Message}");
            return InstallOperationResult.Failed;
        }
    }
}