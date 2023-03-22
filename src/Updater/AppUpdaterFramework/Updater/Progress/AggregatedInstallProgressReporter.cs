using System;
using AnakinRaW.AppUpdaterFramework.Updater.Tasks;

namespace AnakinRaW.AppUpdaterFramework.Updater.Progress;

internal class AggregatedInstallProgressReporter : AggregatedComponentProgressReporter
{
    private readonly object _syncLock = new();


    private long _totalProgressSize;

    protected override ProgressType Type => ProgressType.Install;

    public AggregatedInstallProgressReporter(IProgressReporter progressReporter) : base(progressReporter)
    {
    }

    protected override double CalculateAggregatedProgress(IProgressTask task, double taskProgress, ref ProgressInfo progressInfo)
    {
        lock (_syncLock)
        {
            var totalTaskProgressSize = (long)(taskProgress * task.Size);
            _totalProgressSize += totalTaskProgressSize;
            var totalProgress = (double)_totalProgressSize / TotalSize;
            totalProgress = Math.Min(totalProgress, 1.0);
            totalProgress = totalProgress >= 1.0 ? 0.99 : totalProgress;
            progressInfo.TotalComponents = TotalComponentCount;
            progressInfo.CurrentComponent = 1; // TODO
            return totalProgress;
        }
    }
}