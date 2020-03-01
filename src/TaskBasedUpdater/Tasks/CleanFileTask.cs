using System;
using System.Threading;
using TaskBasedUpdater.Component;
using TaskBasedUpdater.FileSystem;

namespace TaskBasedUpdater.Tasks
{
    internal class CleanFileTask : SynchronizedUpdaterTask
    {
        internal string File { get; set; }

        public CleanFileTask(IComponent component, string filePath)
        {
            Component = component;
            File = filePath;
        }

        public override string ToString()
        {
            return $"Cleaning file '{File}'";
        }

        protected override void SynchronizedInvoke(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;
            if (!FileSystemExtensions.DeleteFileWithRetry(File, out _))
                throw new Exception($"Failed to delete file: {File}");

        }
    }
}
