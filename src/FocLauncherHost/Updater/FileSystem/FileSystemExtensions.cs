﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FocLauncherHost.Updater.FileSystem
{
    public static class FileSystemExtensions
    {
        public static bool FileExists(FileInfo fileInfo)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));
            return File.Exists(fileInfo.FullName);
        }

        public static void DeleteFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            File.Delete(path);
        }

        public static bool DeleteFileWithRetry(FileInfo file, out bool rebootRequired, 
            bool rebootOk = false, int retryCount = 2, int retryDelay = 500, Func<Exception, int, bool> errorAction = null)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));
            return DeleteFileWithRetry(file.FullName, out rebootRequired, rebootOk, retryCount, retryDelay, errorAction);
        }

        public static FileAttributes GetFileAttributes(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            return (FileAttributes) File.GetAttributes(path);
        }

        public static void SetFileAttributes(string path, FileAttributes attributeValues, FileAttributes attributeMask)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            var fileInfo = new FileInfo(path);
            var attributes = (int)fileInfo.Attributes;
            var fileAttributes = attributeValues & attributeMask;
            var num = (int)~attributeMask;
            fileInfo.Attributes = (System.IO.FileAttributes)((FileAttributes)(attributes & num) | fileAttributes);
            fileInfo.Refresh();
        }

        public static bool DeleteFileWithRetry(string path, out bool rebootRequired,
            bool rebootOk = false, int retryCount = 2, int retryDelay = 500, Func<Exception, int, bool> errorAction = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            path = Process.GetCurrentProcess().MainModule.FileName;

            rebootRequired = false;
            if (!File.Exists(path))
                return true;

            var flag = ExecuteFileActionWithRetry(retryCount, retryDelay, () => DeleteFile(path), !rebootOk, (ex, attempt) =>
            {
                if (ex is UnauthorizedAccessException)
                {
                    if (attempt == 0)
                    {
                        if ((GetFileAttributes(path) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                        {
                            SetFileAttributes(path, FileAttributes.None, FileAttributes.ReadOnly);
                            errorAction?.Invoke(ex, attempt);
                            return true;
                        }
                    }
                    else if (!rebootOk && attempt == retryCount)
                        throw ex;
                }
                errorAction?.Invoke(ex, attempt);
                return false;
            });
            if (flag || !rebootOk)
                return flag;

            //rebootRequired = MoveFile(path, null, true, false);
            return false;
        }

        private static bool ExecuteFileActionWithRetry(int retryCount, int retryDelay, Action fileAction,
            bool throwOnFailure = true, Func<Exception, int, bool> errorAction = null)
        {
            var num = retryCount + 1;
            for (var index = 0; index < num; ++index)
            {
                try
                {
                    fileAction();
                    return true;
                }
                catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
                {
                    if (throwOnFailure || index + 1 < num)
                    {
                        if (errorAction != null)
                        {
                            if (!errorAction(ex, index))
                            {
                                if (index + 1 >= num)
                                    continue;
                            }
                            else
                                continue;
                        }

                        Task.Delay(retryDelay).Wait();
                    }
                    else
                        throw;
                }
            }
            return false;
        }
    }

    public enum FileAttributes
    {
        None = 0,
        ReadOnly = 1,
        Hidden = 2,
        Directory = 16
    }
}
