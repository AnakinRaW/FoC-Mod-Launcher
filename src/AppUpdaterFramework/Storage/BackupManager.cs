using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Product;
using AnakinRaW.CommonUtilities.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.Storage;

internal class BackupManager : IBackupManager
{
    private readonly ConcurrentDictionary<IInstallableComponent, BackupValueData> _backups = new(ProductComponentIdentityComparer.Default);
    private readonly IFileSystem _fileSystem;
    private readonly IFileSystemService _fileSystemHelper;
    private readonly ILogger? _logger;
    private readonly BackupRepository _repository;
    private readonly IProductService _productService;

    public IEnumerable<IInstallableComponent> Backups => _backups.Keys;

    public BackupManager(IServiceProvider serviceProvider)
    {
        _productService = serviceProvider.GetRequiredService<IProductService>();
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _fileSystemHelper = serviceProvider.GetRequiredService<IFileSystemService>();
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _repository = serviceProvider.GetRequiredService<BackupRepository>();
    }

    public void BackupComponent(IInstallableComponent component)
    {
        Requires.NotNull(component, nameof(component));
        if (component is not SingleFileComponent singleFileComponent)
            throw new NotSupportedException($"argument '{nameof(component)}' must be of type '{nameof(SingleFileComponent)}'");

        var variables = _productService.GetCurrentInstance().Variables;
        var source = singleFileComponent.GetFile(_fileSystem, variables);

        if (component.DetectedState == DetectionState.Absent)
            return;

        if (!source.Exists)
        {
            var e = new FileNotFoundException("Could not find source file to backup.");
            _logger?.LogError(e, e.Message);
            throw e;
        }

        var backupData = _backups.GetOrAdd(component, _ =>
        {
            var backupFile = _repository.AddComponent(component);
            return new BackupValueData(source, backupFile);
        });

        try
        {
            var backup = backupData.Backup;
            backup.Directory!.Create();
            _fileSystemHelper.CopyFileWithRetry(source, backup.FullName);
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

        if (!_backups.TryRemove(component, out var backup))
            return;

        _fileSystemHelper.CopyFileWithRetry(backup.Backup, backup.Source.FullName);
        _repository.RemoveComponent(component);
    }

    public void RemoveBackup(IInstallableComponent component)
    {
        _backups.TryRemove(component, out _);
        _repository.RemoveComponent(component);
    }

    public void RestoreAll()
    {
        foreach (var component in _backups.Keys) 
            RestoreBackup(component);
    }

    public void RemoveBackups()
    {
        _repository.Clear();
        _backups.Clear();
    }

    private struct BackupValueData
    {
        public IFileInfo Source { get; }

        public IFileInfo Backup { get; }

        public BackupValueData(IFileInfo source, IFileInfo backup)
        {
            Source = source;
            Backup = backup;
        }
    }
}