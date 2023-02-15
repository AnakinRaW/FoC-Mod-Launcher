using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AnakinRaW.AppUpaterFramework.Updater.Tasks;
using Validation;

namespace AnakinRaW.AppUpaterFramework.Updater.Progress;

internal class AggregatedComponentProgressReporter : ITaskProgressReporter
{
    private readonly IProgressReporter _progressReporter;

    private readonly object _syncLock = new();

    private long _totalProgressSize;

    private int _completedPackageCount;
    private long _completedSize;
    private long _completedSizeForSpeedCalculation;
    private long _previousCompletedSizeForSpeedCalculation;

    private readonly IDictionary<string, long>? _progressTable = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
    private DateTime _downloadTime = DateTime.Now;
    private int _reportTimes = 1;
    private long _byteRate;

    private HashSet<IProgressTask>? PackageProgressCollection { get; set; }

    private long TotalSize { get; set; }

    public AggregatedComponentProgressReporter(IProgressReporter progressReporter)
    {
        _progressReporter = progressReporter;
    }

    internal void Initialize(IEnumerable<IProgressTask> progressTasks)
    {
        PackageProgressCollection = new HashSet<IProgressTask>(progressTasks, ProgressTaskComparer.Default);
        TotalSize = PackageProgressCollection.Sum(x => x.Size);
    }

    public void Report(IProgressTask task, double progress, ProgressInfo progressInfo)
    {
        Requires.NotNull(task, nameof(task));

        var actualProgressInfo = new ProgressInfo();
        var currentProgress = 0.0;

        if (TotalSize > 0)
        {
            if (task.Type == ProgressType.Download)
            {
                var now = DateTime.Now;
                var key = task.Component.GetUniqueId();
                var totalTaskProgressSize = (long)(progress * task.Size);
                
                if (progress >= 1.0)
                    Interlocked.Increment(ref _completedPackageCount);

                lock (_syncLock)
                {
                    if (!_progressTable.ContainsKey(key))
                    {
                        _progressTable.Add(key, totalTaskProgressSize);
                        _completedSize += totalTaskProgressSize;
                        _completedSizeForSpeedCalculation += totalTaskProgressSize;
                    }
                    else
                    {
                        var deltaSize = totalTaskProgressSize - _progressTable[key];
                        _progressTable[key] = totalTaskProgressSize;
                        _completedSize += deltaSize;
                        _completedSizeForSpeedCalculation += deltaSize;
                    }

                    if (_completedSize < 0)
                        _completedSize = 0;
                    if (_completedSizeForSpeedCalculation < 0L)
                        _completedSizeForSpeedCalculation = 0L;
                    currentProgress = (double)_completedSize / TotalSize;
                    currentProgress = Math.Min(currentProgress, 1.0);

                    var deltaDownloadSpeed = _completedSizeForSpeedCalculation - _previousCompletedSizeForSpeedCalculation;
                    var totalSeconds = (now - _downloadTime).TotalSeconds;
                    if (totalSeconds > 10.0)
                    {
                        _previousCompletedSizeForSpeedCalculation = _completedSizeForSpeedCalculation;
                        _downloadTime = now;
                    }
                    if (deltaDownloadSpeed >= 0 && totalSeconds != 0.0)
                    {
                        var currentByteRate = (long)(deltaDownloadSpeed / totalSeconds);
                        if (_reportTimes > 1000)
                            _reportTimes = 1;
                        _byteRate += CalculateMovingAverage(currentByteRate, _byteRate, _reportTimes);
                        ++_reportTimes;
                    }
                    actualProgressInfo.DownloadedSize = _completedSize;
                    actualProgressInfo.DownloadSpeed = _byteRate;
                    actualProgressInfo.TotalSize = TotalSize;
                }

                var totalPackageCount = PackageProgressCollection?.Count ?? 0;
                if (_completedPackageCount >= totalPackageCount && progress >= 1.0)
                {
                    currentProgress = 1.0;
                    actualProgressInfo.DownloadSpeed = 0L;
                }
                else
                    currentProgress *= 0.99;
            }
            else if (task.Type == ProgressType.Install)
            {
                lock (_syncLock)
                { 
                    var totalTaskProgressSize = (long)(progress * task.Size);
                     _totalProgressSize += totalTaskProgressSize;
                     var totalProgress = (double)_totalProgressSize / TotalSize;
                     totalProgress = Math.Min(totalProgress, 1.0);
                     currentProgress = totalProgress >= 1.0 ? 0.99 : totalProgress;
                     actualProgressInfo.TotalComponents = PackageProgressCollection?.Count ?? 0;
                     actualProgressInfo.CurrentComponent = 1;
                }
            }
        }

        _progressReporter.Report(task.Component.Id, currentProgress, task.Type, actualProgressInfo);

        static long CalculateMovingAverage(long currentByteRate, long previousByteRate, int reportTimes)
        {
            return previousByteRate + (currentByteRate - previousByteRate) / reportTimes;
        }
    }
}