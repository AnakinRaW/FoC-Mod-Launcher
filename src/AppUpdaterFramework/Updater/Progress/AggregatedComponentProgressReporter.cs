using System.Collections.Generic;
using System.Linq;
using AnakinRaW.AppUpaterFramework.Updater.Tasks;
using Validation;

namespace AnakinRaW.AppUpaterFramework.Updater.Progress;

internal class AggregatedComponentProgressReporter : ITaskProgressReporter
{
    private readonly IProgressReporter _progressReporter;

    private readonly object _syncLock = new();

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

        //var progressTable = new Dictionary<string, long>(PackageProgressCollection.Count, StringComparer.OrdinalIgnoreCase);
        //if (this.PreviousTotalPackageCount > PackageProgressCollection.Count)
        //{
        //    this.totalPackageCount = this.PreviousTotalPackageCount;
        //    this.completedPackageCount = this.PreviousTotalPackageCount - PackageProgressCollection.Count;
        //}
        //else
        //    this.totalPackageCount = PackageProgressCollection.Count;
        //if (this.PreviousTotalSize > TotalSize)
        //{
        //    this.totalPackageSize = this.PreviousTotalSize;
        //    this.completedSize = this.PreviousTotalSize - TotalSize;
        //}
        //else
        //    this.totalPackageSize = TotalSize;

    }

    public void Report(IProgressTask task, double progress, ProgressInfo progressInfo)
    {
        Requires.NotNull(task, nameof(task));

        var actualProgressInfo = new ProgressInfo();

        if (TotalSize > 0)
        {
            if (task.Type == ProgressType.Download)
            {
                //var now = DateTime.Now;
                //var num2 = (long)(progress * task.Size);
                //if (string.IsNullOrEmpty(individualProgressData.PayloadUrl))
                //{
                //    if (progress == 1.0)
                //        Interlocked.Increment(ref this.completedPackageCount);
                //}

                //lock (_syncLock)
                //{
                //    long num4 = num2 - this.progressTable[key];
                //    long num5 = 0;
                //    if (task.Size < num2)
                //        num5 = this.progressTable[key] <= packageProgress.Size ? num2 - packageProgress.Size : num4;
                //    this.TotalSize += num5;
                //    this.totalPackageSize += num5;
                //    this.progressTable[key] = num2;
                //    this.completedSize += num4;
                //    this.completedSizeForSpeedCalculation += num4;

                //    if (this.completedSize < 0L)
                //        this.completedSize = 0L;
                //    if (this.completedSizeForSpeedCalculation < 0L)
                //        this.completedSizeForSpeedCalculation = 0L;
                //    num1 = (double)this.completedSize / (double)this.totalPackageSize;
                //    num1 = Math.Min(num1, 1.0);
                //    long num6 = this.completedSizeForSpeedCalculation - this.previousCompletedSizeForSpeedCalculation;
                //    double totalSeconds = (now - this.downloadTime).TotalSeconds;
                //    if (totalSeconds > 10.0)
                //    {
                //        this.previousCompletedSizeForSpeedCalculation = this.completedSizeForSpeedCalculation;
                //        this.downloadTime = now;
                //    }
                //    if (num6 >= 0L && totalSeconds != 0.0)
                //    {
                //        long currentByteRate = (long)((double)num6 / totalSeconds);
                //        if (this.reportTimes > 1000)
                //            this.reportTimes = 1;
                //        this.byteRate = ProgressAggregator.CalculateMovingAverage(currentByteRate, this.byteRate, this.reportTimes);
                //        ++this.reportTimes;
                //    }
                //    progressInfo.DownloadedSize = this.completedSize;
                //    progressInfo.DownloadSpeed = this.byteRate;
                //    progressInfo.TotalSize = this.totalPackageSize;
                //}

                //if (this.completedPackageCount >= this.totalPackageCount && individualProgressData.Progress == 1.0)
                //{
                //    num1 = 1.0;
                //    progressInfo.DownloadSpeed = 0L;
                //}
                //else
                //    num1 *= 0.99;
            }
            else if (task.Type == ProgressType.Install)
            {
                lock (_syncLock)
                {
                    var uniqueId = task.Component.GetUniqueId();
                    long totalTaskProgressSize = (long)(progress * task.Size);
                    //this.progressTable[uniqueId] = totalTaskProgress;
                    //num1 = (double)(this.progressTable.Values.Sum() + this.completedSize) / (double)this.totalPackageSize;
                    //num1 = Math.Min(num1, 1.0);
                    //num1 = num1 >= 1.0 ? 0.99 : num1;
                    //progressInfo.TotalComponents = this.totalPackageCount;
                    //progressInfo.CurrentComponent = this.progressTable.Count + this.completedPackageCount;
                }
            }
        }

        _progressReporter.Report(task.Component.Id, progress, task.Type, actualProgressInfo);
    }
}