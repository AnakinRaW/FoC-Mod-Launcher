using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using AnakinRaW.AppUpdaterFramework.Conditions;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Metadata.Update;
using AnakinRaW.AppUpdaterFramework.Product;
using AnakinRaW.AppUpdaterFramework.Restart;
using AnakinRaW.AppUpdaterFramework.Storage;
using AnakinRaW.AppUpdaterFramework.Utilities;
using AnakinRaW.ExternalUpdater;
using AnakinRaW.ExternalUpdater.Options;
using AnakinRaW.ExternalUpdater.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.AppUpdaterFramework.ExternalUpdater;

internal class ExternalUpdaterService : IExternalUpdaterService
{
    private const string Identity = "ExternalUpdater";
    private const string ComponentName = "External Updater";

    private readonly IFileSystem _fileSystem;
    private readonly IProductService _productService;
    private readonly IExternalUpdaterLauncher _launcher;
    private readonly IPendingComponentStore _pendingComponentStore;
    private readonly IBackupManager _backupManager;
    private readonly DownloadRepository _downloadRepository;

    public string UpdaterIdentity => Identity;

    public ExternalUpdaterService(IServiceProvider serviceProvider)
    {
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _productService = serviceProvider.GetRequiredService<IProductService>();
        _launcher = serviceProvider.GetRequiredService<IExternalUpdaterLauncher>();
        _pendingComponentStore = serviceProvider.GetRequiredService<IPendingComponentStore>();
        _backupManager = serviceProvider.GetRequiredService<IBackupManager>();
        _downloadRepository = serviceProvider.GetRequiredService<DownloadRepository>();
    }

    public IInstallableComponent GetExternalUpdaterComponent(Stream assemblyStream, string installDirectory)
    {
        var assemblyInformation = ExternalUpdaterInformation.FromAssemblyStream(assemblyStream);
       
        var fileVersion = assemblyInformation.FileVersion;
        var filePath = _fileSystem.Path.Combine(installDirectory, ExternalUpdaterConstants.AppUpdaterModuleName);
        var identity = new ProductComponentIdentity(UpdaterIdentity, assemblyInformation.InformationalVersion);

        return new SingleFileComponent(identity, installDirectory, ExternalUpdaterConstants.AppUpdaterModuleName, null)
        {
            Name = ComponentName,
            DetectConditions = new[]
            {
                new FileCondition(filePath)
                {
                    Version = fileVersion
                }
            }
        };
    }

    public UpdateOptions CreateUpdateOptions()
    {
        var cpi = CurrentProcessInfo.Current;

        var updateInformationFile = WriteToTempFile(CollectUpdateInformation());

        return new UpdateOptions
        {
            AppToStart = cpi.ProcessFilePath,
            Pid = cpi.Id,
            UpdateFile = updateInformationFile
        };
    }

    public RestartOptions CreateRestartOptions(bool elevate)
    {
        var cpi = CurrentProcessInfo.Current;
        return new RestartOptions
        {
            AppToStart = cpi.ProcessFilePath,
            Pid = cpi.Id,
            Elevate = elevate
        };
    }

    public IFileInfo GetExternalUpdater()
    {
        if (_productService.GetInstalledComponents().Items.FirstOrDefault(c => c.Id == UpdaterIdentity) is not SingleFileComponent updaterComponent)
            throw new NotSupportedException("External updater component not registered to current product.");

        return updaterComponent.GetFile(_fileSystem, _productService.GetCurrentInstance().Variables);
    }

    public void Launch(ExternalUpdaterOptions options)
    {
        var updater = GetExternalUpdater();
        _launcher.Start(updater, options);
    }

    private IEnumerable<UpdateInformation> CollectUpdateInformation()
    {
        var pendingComponents = _pendingComponentStore.PendingComponents;
        var backups = _backupManager.Backups;

        var updateInformation = new List<UpdateInformation>();

        foreach (var pendingComponent in pendingComponents)
        {
            //if (pendingComponent.Action == UpdateAction.Keep)
            //    continue;

            //BackupInformation? backupInformation = null;
            //if (backups.TryGetValue(pendingComponent, out var backup))
            //{
            //    backupInformation = CreateFromBackup(backup);
            //    backups.Remove(pendingComponent);
            //}

            //var copyInformation = CreateFromComponent(pendingComponent);
            
            //var item = new UpdateInformation
            //{
            //    Update = copyInformation,
            //    Backup = backupInformation
            //};

            //updateInformation.Add(item);
        }

        foreach (var backup in backups.Values)
        {
            var backupInformation = CreateFromBackup(backup);
            var item = new UpdateInformation
            {
                Backup = backupInformation
            };
            updateInformation.Add(item);
        }

        return updateInformation;
    }

    private string WriteToTempFile(IEnumerable<UpdateInformation> updateInformation)
    {
        var tempPath = _fileSystem.Path.GetTempPath();
        var fileName = _fileSystem.Path.GetTempFileName();
        var tempFilePath = _fileSystem.Path.Combine(tempPath, fileName);
        
        using var fs = _fileSystem.FileStream.New(tempFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        using var writer = new StreamWriter(fs);
        writer.Write(updateInformation.Serialize());

        return tempFilePath;
    }

    private BackupInformation CreateFromBackup(BackupValueData backup)
    {
        return new BackupInformation
        {
            Destination = "",
            Source = ""
        };
    }

    private FileCopyInformation CreateFromComponent(IInstallableComponent component, UpdateAction action)
    {
        if (action == UpdateAction.Keep)
            throw new NotSupportedException("UpdateAction Keep is not supported");

        string componentLocation = null!; // TODO

        string? destination;
        string source;

        switch (action)
        {
            case UpdateAction.Update:
                source = _downloadRepository.GetComponent(component)?.FullName ??
                         throw new InvalidOperationException(
                             $"Unable to find source location for component: {component}");
                destination = componentLocation;
                break;
            case UpdateAction.Delete:
                source = componentLocation;
                destination = null;
                break;
            case UpdateAction.Keep:
                throw new NotSupportedException("UpdateAction Keep is not supported");
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
        return new FileCopyInformation
        {
            Destination = destination,
            File = source
        };
    }
}