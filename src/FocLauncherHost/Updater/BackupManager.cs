﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using FocLauncherHost.Updater.Component;
using FocLauncherHost.Updater.FileSystem;
using FocLauncherHost.Updater.Restart;

namespace FocLauncherHost.Updater
{
    public class BackupManager
    {
        private static BackupManager _instance;

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
            var backupFilePath = CreateBackupFilePath(component, backupPath);
            var componentFilePath = component.GetFilePath();
            FileSystemExtensions.CopyFileWithRetry(componentFilePath, backupFilePath);
            _backupLookup.Add(component, backupFilePath);
        }

        public void RemoveAllBackups()
        {
            var keys = _backupLookup.Keys.ToList();
            foreach (var component in keys) 
                RemoveBackup(component);
        }

        public void RestoreAllBackups()
        {
            var keys = _backupLookup.Keys.ToList();
            foreach (var component in keys)
                RestoreBackup(component);
        }

        private void RestoreBackup(IComponent component)
        {
            if (!_backupLookup.ContainsKey(component))
                return;
            var backupFile = _backupLookup[component];
            try
            {
                if (!File.Exists(backupFile))
                    return;

                var componentFile = component.GetFilePath();

                if (File.Exists(componentFile))
                {
                    var backupHash = UpdaterUtilities.GetFileHash(backupFile, HashType.Sha2);
                    var fileHash = UpdaterUtilities.GetFileHash(backupFile, HashType.Sha2);
                    if (backupHash.SequenceEqual(fileHash))
                        return;
                }
                var success = FileSystemExtensions.MoveFile(backupFile, component.GetFilePath(), true);
                if (!success)
                    throw new IOException("Unable to restore the backup file. Please restart your computer and try again.");
            }
            finally
            {
                FileSystemExtensions.DeleteFileWithRetry(backupFile, out _);
                _backupLookup.Remove(component);
            }
        }

        private void RemoveBackup(IComponent component)
        {
            if (!_backupLookup.ContainsKey(component))
                return;
            var backupFile = _backupLookup[component];
            try
            {
                if (!File.Exists(backupFile))
                    return;
                FileSystemExtensions.DeleteFileWithRetry(backupFile, out _);
            }
            finally
            {
                _backupLookup.Remove(component);
            }
        }

        internal static void ValidateComponent(IComponent component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));
            if (string.IsNullOrEmpty(component.Destination))
                throw new FileNotFoundException("Unable to resolve the component's file path");
            var filePath = Path.Combine(component.Destination, component.Name);
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Unable to resolve the component's file path");
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
