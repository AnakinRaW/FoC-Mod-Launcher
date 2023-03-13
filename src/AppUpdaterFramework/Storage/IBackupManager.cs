using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Product;
using AnakinRaW.AppUpdaterFramework.Updater.Configuration;
using AnakinRaW.CommonUtilities.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.Storage;

internal interface IBackupManager
{
    void BackupComponent(IInstallableComponent component);

    void RestoreBackup(IInstallableComponent component);

    void RestoreAll();

    void RemoveBackups();
}

internal class BackupManager : IBackupManager
{
    private readonly IInstalledProduct _currentInstance;

    private readonly ConcurrentDictionary<IInstallableComponent, BackupValueData> _backups = new(ProductComponentIdentityComparer.Default);
    private readonly IFileSystem _fileSystem;
    private readonly IFileSystemService _fileSystemHelper;
    private readonly IUpdateConfiguration _updateConfiguration;
    private readonly ILogger? _logger;

    public BackupManager(IServiceProvider serviceProvider)
    {
        _currentInstance = serviceProvider.GetRequiredService<IProductService>().GetCurrentInstance();
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _fileSystemHelper = serviceProvider.GetRequiredService<IFileSystemService>();
        _updateConfiguration = serviceProvider.GetRequiredService<IUpdateConfigurationProvider>().GetConfiguration();
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public void BackupComponent(IInstallableComponent component)
    {
        Requires.NotNull(component, nameof(component));
        if (component is not SingleFileComponent singleFileComponent)
            throw new NotSupportedException($"argument '{nameof(component)}' must be of type '{nameof(SingleFileComponent)}'");


        var filePath = singleFileComponent.GetFile(_fileSystem, _currentInstance.Variables);

        if (component.DetectedState == DetectionState.Absent)
            return;

        if (!filePath.Exists)
        {
            var e = new FileNotFoundException("Could not find source file to backup.");
            _logger?.LogError(e, e.Message);
            throw e;
        }

        var backupData = _backups.GetOrAdd(component, _ =>
        {
            var backupPath = CreateBackupFileInfo(singleFileComponent);
            return new BackupValueData(filePath.FullName, backupPath);
        });

        if (_fileSystem.File.Exists(backupData.BackupPath))
            return;

        try
        {
            var backupPath = backupData.BackupPath;
            _fileSystem.Directory.CreateDirectory(_fileSystem.Path.GetDirectoryName(backupPath)!);
            _fileSystemHelper.CopyFileWithRetry(filePath, backupPath);
        }
        catch (Exception)
        {
            _backups.TryRemove(component, out _);
            throw;
        }
    }

    public void RestoreBackup(IInstallableComponent component)
    {
        Requires.NotNull(component, nameof(component));
        if (component is not SingleFileComponent singleFileComponent)
            throw new NotSupportedException($"argument '{nameof(component)}' must be of type '{nameof(SingleFileComponent)}'");
    }

    public void RestoreAll()
    {
    }

    public void RemoveBackups()
    {
    }


    private string CreateBackupFileInfo(SingleFileComponent component)
    {
        var randomFileName = _fileSystem.Path.GetFileNameWithoutExtension(_fileSystem.Path.GetRandomFileName());
        var backupFileName = $"{component.FileName}.{randomFileName}.bak";

        var backupPath = _updateConfiguration.BackupLocation;
        return _fileSystem.Path.Combine(backupPath, backupFileName);
    }


    private struct BackupValueData
    {
        public string SourcePath { get; }

        public string BackupPath { get; }

        public BackupValueData(string sourcePath, string backupPath)
        {
            SourcePath = sourcePath;
            BackupPath = backupPath;
        }
    }
}