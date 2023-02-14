using System;
using System.Collections.Generic;
using AnakinRaW.AppUpaterFramework.Updater.Tasks;

namespace AnakinRaW.AppUpaterFramework.Updater.Progress;

internal class AggregatedComponentProgressReporter : IDisposable, ITaskProgressReporter
{
    private readonly IProgressReporter _progressReporter;
    private readonly IServiceProvider _serviceProvider;

    public AggregatedComponentProgressReporter(IProgressReporter progressReporter, IServiceProvider serviceProvider)
    {
        _progressReporter = progressReporter;
        _serviceProvider = serviceProvider;
    }

    public void StartReporting(IEnumerable<IProgressTask> progressTasks)
    {
    }

    public void Dispose()
    {
    }

    public void Report(IProgressTask task)
    {
        throw new NotImplementedException();
    }
}