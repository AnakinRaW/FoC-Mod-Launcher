using System;
using System.Threading;
using AnakinRaW.AppUpaterFramework.Metadata.Component;
using AnakinRaW.AppUpaterFramework.Updater.Progress;
using AnakinRaW.CommonUtilities.TaskPipeline.Tasks;

namespace AnakinRaW.AppUpaterFramework.Updater.Tasks;

internal class DownloadTask : SynchronizedTask, IProgressTask
{
    public long Weight { get; }
    public long Size { get; }
    public ProgressType Type { get; }

    public IProductComponent Component { get; }

    public DownloadTask(IInstallableComponent installable, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Component = installable;
    }

    protected override void SynchronizedInvoke(CancellationToken token)
    {
    }

    public ITaskProgressReporter ProgressReporter { get; }
}