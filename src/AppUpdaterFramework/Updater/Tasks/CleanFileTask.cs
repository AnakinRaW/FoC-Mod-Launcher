using System;
using System.Threading;
using AnakinRaW.CommonUtilities.TaskPipeline.Tasks;

namespace AnakinRaW.AppUpdaterFramework.Updater.Tasks;

internal class CleanFileTask : SynchronizedTask
{
    internal string File { get; set; }

    public CleanFileTask(string filePath, IServiceProvider serviceProvider) : base(serviceProvider)
    {
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