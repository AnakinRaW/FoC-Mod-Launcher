using System.Collections.Generic;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;
using AnakinRaW.AppUpdaterFramework.Updater.Tasks;
using Validation;

namespace AnakinRaW.AppUpdaterFramework.Updater.Progress;

internal abstract class AggregatedComponentProgressReporter : ITaskProgressReporter
{
    protected abstract ProgressType Type { get; }

    private readonly IProgressReporter _progressReporter;

    private readonly HashSet<IProgressTask> _componentProgressCollection = new(ProgressTaskComparer.Default);

    protected long TotalSize { get; private set; }

    protected int TotalComponentCount => _componentProgressCollection.Count;

    protected AggregatedComponentProgressReporter(IProgressReporter progressReporter)
    {
        _progressReporter = progressReporter;
    }

    internal void Initialize(IEnumerable<IProgressTask> progressTasks)
    {
        foreach (var task in progressTasks)
        {
            _componentProgressCollection.Add(task);
            TotalSize += task.Size;
        }
    }

    public void Report(IProgressTask task, double progress)
    {
        Requires.NotNull(task, nameof(task));

        if (!_componentProgressCollection.Contains(task))
            return;

        var actualProgressInfo = new ProgressInfo();
        var currentProgress = 0.0;

        if (TotalSize > 0) 
            currentProgress = CalculateAggregatedProgress(task, progress, ref actualProgressInfo);
        
        _progressReporter.Report(task.Component.GetDisplayName(), currentProgress, Type, actualProgressInfo);
    }

    protected abstract double CalculateAggregatedProgress(IProgressTask task, double taskProgress, ref ProgressInfo progressInfo);
}