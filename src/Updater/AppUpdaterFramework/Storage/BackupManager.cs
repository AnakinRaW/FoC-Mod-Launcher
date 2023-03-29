using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Product;
using AnakinRaW.CommonUtilities.FileSystem;
using AnakinRaW.CommonUtilities.Hashing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;
#if NET6_0
using System.Diagnostics.CodeAnalysis;
#endif

namespace AnakinRaW.AppUpdaterFramework.Storage;

internal class BackupManager : IBackupManager
{
    private readonly ConcurrentDictionary<IInstallableComponent, BackupValueData> _backups = new(ProductComponentIdentityComparer.Default);
    private readonly IFileSystem _fileSystem;
    private readonly IFileSystemService _fileSystemHelper;
    private readonly ILogger? _logger;
    private readonly BackupRepository _repository;
    private readonly IProductService _productService;
    private readonly IHashingService _hashingService;

    public IDictionary<IInstallableComponent, BackupValueData> Backups => new Dictionary<IInstallableComponent, BackupValueData>(_backups);

    public BackupManager(IServiceProvider serviceProvider)
    {
        _productService = serviceProvider.GetRequiredService<IProductService>();
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _fileSystemHelper = serviceProvider.GetRequiredService<IFileSystemService>();
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _repository = serviceProvider.GetRequiredService<BackupRepository>();
        _hashingService = serviceProvider.GetRequiredService<IHashingService>();
    }

    public void BackupComponent(IInstallableComponent component)
    {
        Requires.NotNull(component, nameof(component));

        var backupData = _backups.GetOrAdd(component, CreateBackupEntry);

        // Check whether the component is actually present
        if (backupData.IsOriginallyMissing())
            return;

        try
        {
            var backup = backupData.Backup;
            backup!.Directory!.Create();
            _fileSystemHelper.CopyFileWithRetry(backupData.Source, backup.FullName);
        }
        catch (Exception)
        {
            RemoveBackup(component);
            throw;
        }
    }


    public void RestoreBackup(IInstallableComponent component)
    {
        Requires.NotNull(component, nameof(component));

        if (!_backups.TryRemove(component, out var backupData))
            return;

        var source = backupData.Source;

        source.Refresh();

        if (backupData.IsOriginallyMissing())
        {
            if (!source.Exists)
                return;
            if (_fileSystemHelper.DeleteFileWithRetry(source))
                return;
            throw new IOException("Unable to restore the backup. Please restart your computer!");
        }

        var backup = backupData.Backup;
        backup.Refresh();
        if (!backup.Exists)
            throw new FileNotFoundException("Source file not found", backup.FullName);

        try
        {
            if (source.Exists)
            {
                var backHash = _hashingService.GetFileHash(backup, HashType.Sha256);
                var sourceHash = _hashingService.GetFileHash(source, HashType.Sha256);
                if (backHash.SequenceEqual(sourceHash))
                    return;
            }

            _fileSystemHelper.CopyFileWithRetry(backupData.Backup, backupData.Source.FullName);
        }
        finally
        {
            _repository.RemoveComponent(component);
        }
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

    private BackupValueData CreateBackupEntry(IInstallableComponent component)
    {
        if (component is not SingleFileComponent singleFileComponent)
            throw new NotSupportedException($"option '{nameof(component)}' must be of type '{nameof(SingleFileComponent)}'");

        var variables = _productService.GetCurrentInstance().Variables;
        var source = singleFileComponent.GetFile(_fileSystem, variables);

        if (component.DetectedState == DetectionState.Absent)
            return new BackupValueData(source);

        if (!source.Exists)
        {
            var e = new FileNotFoundException("Could not find source file to backup.");
            _logger?.LogError(e, e.Message);
            throw e;
        }

        var backupFile = _repository.AddComponent(component);
        return new BackupValueData(source, backupFile);
    }
}

public class BackupValueData : IEquatable<BackupValueData>
{
    public IFileInfo Source { get; }

    public IFileInfo? Backup { get; }

    public BackupValueData(IFileInfo source)
    {
        Source = source;
        Backup = null;
    }

    public BackupValueData(IFileInfo source, IFileInfo backup)
    {
        Source = source;
        Backup = backup;
    }

#if NET
        [MemberNotNullWhen(false, nameof(Source))]
#endif
    public bool IsOriginallyMissing()
    {
        return Backup is null;
    }

    public bool Equals(BackupValueData other)
    {
        return Source.Equals(other.Source) && Equals(Backup, other.Backup);
    }

    public override bool Equals(object? obj)
    {
        return obj is BackupValueData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Source, Backup);
    }
}