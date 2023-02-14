using AnakinRaW.AppUpaterFramework.Updater.Tasks;

namespace AnakinRaW.AppUpaterFramework.Updater.Progress;

internal interface ITaskProgressReporter
{
    void Report(IProgressTask task);
}