using System;
using AnakinRaW.AppUpdaterFramework.Updater.Tasks;
using AnakinRaW.CommonUtilities.SimplePipeline.Progress;

namespace AnakinRaW.AppUpdaterFramework.Updater.Progress;

internal class AggregatedInstallProgressReporter : ComponentAggregatedProgressReporter
{
    private readonly object _syncLock = new();

    private long _totalProgressSize;

    protected override ProgressType Type => ProgressTypes.Install;

    public AggregatedInstallProgressReporter(IComponentProgressReporter progressReporter) : base(progressReporter)
    {
    }
    
    protected override double CalculateAggregatedProgress(IComponentStep step, double taskProgress, ref ComponentProgressInfo progressInfo)
    {
        lock (_syncLock)
        {
            var totalTaskProgressSize = (long)(taskProgress * step.Size);
            _totalProgressSize += totalTaskProgressSize;
            var totalProgress = (double)_totalProgressSize / TotalSize;
            totalProgress = Math.Min(totalProgress, 1.0);
            totalProgress = totalProgress >= 1.0 ? 0.99 : totalProgress;
            progressInfo.TotalComponents = TotalStepCount;
            progressInfo.CurrentComponent = 1; // TODO
            return totalProgress;
        }
    }
}