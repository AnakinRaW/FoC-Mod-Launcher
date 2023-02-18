using System;
using System.Collections.Generic;
using System.Threading;
using AnakinRaW.AppUpaterFramework.Updater.Tasks;

namespace AnakinRaW.AppUpaterFramework.Updater.Progress;

internal class AggregatedDownloadProgressReporter : AggregatedComponentProgressReporter
{
    private readonly object _syncLock = new();
    private readonly IDictionary<string, long> _progressTable = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

    private int _reportTimes = 1;
    private int _completedPackageCount;
    private long _completedSize;
    private long _completedSizeForSpeedCalculation;
    private long _previousCompletedSizeForSpeedCalculation;
    private long _byteRate;
    private DateTime _downloadTime = DateTime.Now;

    protected override ProgressType Type => ProgressType.Download;

    public AggregatedDownloadProgressReporter(IProgressReporter progressReporter) : base(progressReporter)
    {
    }

    protected override double CalculateAggregatedProgress(IProgressTask task, double taskProgress, ref ProgressInfo progressInfo)
    {
        var now = DateTime.Now;
        var key = task.Component.GetUniqueId();
        var totalTaskProgressSize = (long)(taskProgress * task.Size);

        if (taskProgress >= 1.0)
            Interlocked.Increment(ref _completedPackageCount);

        double currentProgress;

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
            if (_completedSizeForSpeedCalculation < 0)
                _completedSizeForSpeedCalculation = 0;

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
            progressInfo.DownloadedSize = _completedSize;
            progressInfo.DownloadSpeed = _byteRate;
            progressInfo.TotalSize = TotalSize;
        }

        if (_completedPackageCount >= TotalComponentCount && taskProgress >= 1.0)
        {
            currentProgress = 1.0;
            progressInfo.DownloadSpeed = 0L;
        }
        else
            currentProgress *= 0.99;

        return currentProgress;
    }

    private static long CalculateMovingAverage(long currentByteRate, long previousByteRate, int reportTimes)
    {
        return previousByteRate + (currentByteRate - previousByteRate) / reportTimes;
    }
}