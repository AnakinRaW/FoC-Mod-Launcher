using System;
using System.IO;
using System.Threading;
using FocLauncherHost.Updater.FileSystem;

namespace FocLauncherHost.Updater.Tasks
{
    internal sealed class RemoveFileTask : SynchronizedUpdaterTask
    {
        public FileInfo File { get; }

        public bool RebootOk { get; }

        public RemoveFileTask(FileInfo file, bool rebootOk = false)
        {
            File = file;
            RebootOk = rebootOk;
        }

        public override string ToString()
        {
            return $"Deleting file: {File}";
        }

        protected override void SynchronizedInvoke(CancellationToken token)
        {
            if (File == null)
                return;
            if (!FileSystemExtensions.FileExists(File))
                return;
            //try
            //{
            //    if (FileSystemExtensions.DeleteFileWithRetry(File, out bool rebootRequired, RebootOk))
            //        Logger.Debug($"{File} file deleted successfully.");
            //    else if (rebootRequired)
            //    {
            //        // TODO
            //    }
            //    else
            //        Logger.Warn($"{File} file could not be deleted nor was it scheduled for deleteion after reboot.");
            //}
            //catch (Exception e)
            //{
            //    Logger.Error(e, $"Failed to delete file '{File}': {e.Message}");
            //}
        }
    }
}