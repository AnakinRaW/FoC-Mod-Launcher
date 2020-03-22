using System;
using System.IO;
using System.Threading.Tasks;
using FocLauncher.AppUpdater.NativeMethods;

namespace FocLauncher.AppUpdater
{
    internal class FileUtilities
    { 
        internal static bool DeleteFileWithRetry(string path, bool throwException = false, int retryCount = 2, int retryDelay = 500, Func<Exception, int, bool> errorAction = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path))
                return true;

            var flag = ExecuteFileActionWithRetry(retryCount, retryDelay, () => DeleteFile(path), throwException, (ex, attempt) =>
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
                    else if (throwException && attempt == retryCount)
                        throw ex;
                }
                errorAction?.Invoke(ex, attempt);
                return false;
            });
            if (flag || throwException)
                return flag;

            return false;
        }

        internal static bool MoveFile(string source, string destination, bool replace = false)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            var isDirectory = Directory.Exists(source);
            if (!replace)
            {
                if (isDirectory)
                    Directory.Move(source, destination);
                else
                    File.Move(source, destination);
                return true;
            }
            var flags = Kernel32.MoveFileFlags.MoveFileWriteThrough;
            flags |= Kernel32.MoveFileFlags.MoveFileReplaceExisting;
            if (!isDirectory)
                flags |= Kernel32.MoveFileFlags.MoveFileCopyAllowed;
            return Kernel32.MoveFileEx(source, destination, flags);
        }

        private static FileAttributes GetFileAttributes(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            return (FileAttributes)File.GetAttributes(path);
        }

        private static void SetFileAttributes(string path, FileAttributes attributeValues, FileAttributes attributeMask)
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

        private static void DeleteFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            File.Delete(path);
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
                    if (!throwOnFailure || index + 1 < num)
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

        public enum FileAttributes
        {
            None = 0,
            ReadOnly = 1,
            Hidden = 2,
            Directory = 16
        }
    }
}