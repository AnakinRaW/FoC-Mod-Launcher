using System;
using System.Threading;
using System.Threading.Tasks;
using AnakinRaW.AppUpaterFramework.Metadata.Component;
using AnakinRaW.AppUpaterFramework.Updater.Progress;
using AnakinRaW.CommonUtilities.TaskPipeline.Tasks;

namespace AnakinRaW.AppUpaterFramework.Updater.Tasks;

internal class DownloadTask : SynchronizedTask, IProgressTask
{
    public ITaskProgressReporter ProgressReporter { get; }
    public ProgressType Type => ProgressType.Download;
    public long Size { get; }

    public IProductComponent Component { get; }

    public DownloadTask(IInstallableComponent installable, ITaskProgressReporter progressReporter, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Component = installable;
        ProgressReporter = progressReporter;
        Size = installable.DownloadSize;
    }

    protected override void SynchronizedInvoke(CancellationToken token)
    {
        for (int i = 0; i < 100; i++)
        {
            ProgressReporter.Report(this, (double)i / 100, new ProgressInfo());
            Task.Delay(25, token).Wait(token);
        }
    }
}