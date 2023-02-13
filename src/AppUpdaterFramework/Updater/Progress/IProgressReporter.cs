namespace AnakinRaW.AppUpaterFramework.Updater.Progress;

internal interface IProgressReporter
{
    void Report(string package, double progress, ProgressType type, ProgressInfo detailedProgress);
}