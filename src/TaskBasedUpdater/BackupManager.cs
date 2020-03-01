using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.Configuration;
using TaskBasedUpdater.FileSystem;

namespace TaskBasedUpdater
{
    public class BackupManager : IEnumerable<KeyValuePair<IComponent, string>>
    {
        private const string NonExistentSource = "SOURCE_ORIGINALLY_MISSING";

        private static BackupManager _instance;

        private readonly object _syncObject = new object();
        private readonly Dictionary<IComponent, string> _backupLookup = new Dictionary<IComponent, string>();

        public static BackupManager Instance => _instance ??= new BackupManager();

        private BackupManager()
        {
        }

        public void CreateBackup(IComponent component)
        {
            ValidateComponent(component);
            var backupPath = GetBackupPath(component);
            ValidateHasAccess(backupPath);
            if (_backupLookup.ContainsKey(component))
                return;
            string backupFilePath;
            var componentFilePath = component.GetFilePath();
            if (File.Exists(componentFilePath))
            {
                backupFilePath = CreateBackupFilePath(component, backupPath);
                FileSystemExtensions.CopyFileWithRetry(componentFilePath, backupFilePath);
            }
            else
            {
                backupFilePath = NonExistentSource;
            }
            lock (_syncObject)
                _backupLookup.Add(component, backupFilePath);
        }

        public void RestoreAllBackups()
        {
            var keys = _backupLookup.Keys.ToList();
            foreach (var component in keys)
                RestoreBackup(component);
        }
        
        public void RestoreBackup(IComponent component)
        {
            if (!_backupLookup.ContainsKey(component))
                return;
            var backupFile = _backupLookup[component];
            var componentFile = component.GetFilePath();

            var remove = true;

            try
            {
                if (backupFile.Equals(NonExistentSource))
                {
                    if (!File.Exists(componentFile))
                        return;
                    var success = FileSystemExtensions.DeleteFileWithRetry(componentFile, out _);
                    if (success) 
                        return;
                    remove = false;
                    throw new IOException("Unable to restore the backup. Please restart your computer!");
                }
                else
                {
                    if (!File.Exists(backupFile))
                        return;

                    if (File.Exists(componentFile))
                    {
                        var backupHash = UpdaterUtilities.GetFileHash(backupFile, HashType.Sha256);
                        var fileHash = UpdaterUtilities.GetFileHash(backupFile, HashType.Sha256);
                        if (backupHash.SequenceEqual(fileHash))
                        {
                            remove = false;
                            return;
                        }
                    }
                    var success = FileSystemExtensions.MoveFile(backupFile, component.GetFilePath(), true);
                    if (!success)
                    {
                        remove = false;
                        throw new IOException($"Unable to restore the backup file '{backupFile}'. Please restart your computer!");
                    }

                    if (UpdateConfiguration.Instance.DownloadOnlyMode)
                        ComponentDownloadPathStorage.Instance.Remove(component);

                    try
                    {
                        FileSystemExtensions.DeleteFileWithRetry(backupFile, out _);
                    }
                    catch
                    {
                        remove = false;
                    }
                }
            }
            finally
            {
                if (remove)
                    lock (_syncObject)
                        _backupLookup.Remove(component);
            }
        }

        public void Flush()
        {
            _backupLookup.Clear();
        }

        public IEnumerator<KeyValuePair<IComponent, string>> GetEnumerator()
        {
            return _backupLookup.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal static void ValidateComponent(IComponent component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));
            if (string.IsNullOrEmpty(component.Destination))
                throw new IOException("Unable to resolve the component's file path");
        }

        internal static void ValidateHasAccess(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            if (!FileSystemExtensions.UserHasDirectoryAccessRights(path, FileSystemRights.Read | FileSystemRights.Write, true))
                throw new InvalidOperationException($"No Read/Write access to the backup directory: {path}");
        }

        private static string CreateBackupFilePath(IComponent component, string backupPath)
        {
            var randomFileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            var backupFileName = $"{component.Name}.{randomFileName}.bak";
            return Path.Combine(backupPath, backupFileName);
        }

        private static string GetBackupPath(IComponent component)
        {
            var backupPath = UpdateConfiguration.Instance.BackupPath;
            if (string.IsNullOrEmpty(backupPath))
                backupPath = component.Destination;
            return backupPath;
        }
    }
}
