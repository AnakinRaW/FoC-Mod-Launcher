using AnakinRaW.AppUpdaterFramework.Updater.Tasks;

namespace AnakinRaW.AppUpdaterFramework.Updater.Progress;

internal interface ITaskProgressReporter
{
    void Report(IProgressTask task, double progress);
}