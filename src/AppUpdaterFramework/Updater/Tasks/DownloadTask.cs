using System;
using System.Threading;
using AnakinRaW.AppUpaterFramework.Metadata.Component;
using AnakinRaW.AppUpaterFramework.Updater.Progress;
using AnakinRaW.CommonUtilities.TaskPipeline.Tasks;

namespace AnakinRaW.AppUpaterFramework.Updater.Tasks;

internal class DownloadTask : SynchronizedTask, IProgressTask
{
    public ITaskProgressReporter ProgressReporter { get; }
    public ProgressType Type => ProgressType.Download;
    public long Size { get; }

    public Uri Uri { get; }

    public IProductComponent Component { get; }

    public DownloadTask(IInstallableComponent installable, ITaskProgressReporter progressReporter, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Component = installable;
        ProgressReporter = progressReporter;
        Size = installable.DownloadSize;
        Uri = installable.OriginInfo!.Url;
    }

    public override string ToString()
    {
        return $"Downloading component '{Component.GetUniqueId()}' form \"{Uri}\"";
    }

    protected override void SynchronizedInvoke(CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return;
        try
        {
            ReportProgress(0.0);
        }
        finally
        {
            ReportProgress(1.0);
        }
    }

    private void ReportProgress(double progress)
    {
        ProgressReporter.Report(this, progress);
    }
}