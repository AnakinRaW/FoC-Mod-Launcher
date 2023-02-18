using AnakinRaW.AppUpaterFramework.Updater.Progress;

namespace AnakinRaW.AppUpaterFramework.Updater.Tasks;

internal interface IProgressTask : IComponentTask
{
    public ITaskProgressReporter ProgressReporter { get; }

    long Size { get; }
}