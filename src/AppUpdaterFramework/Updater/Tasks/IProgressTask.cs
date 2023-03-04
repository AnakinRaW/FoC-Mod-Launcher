using AnakinRaW.AppUpaterFramework.Updater.Progress;

namespace AnakinRaW.AppUpaterFramework.Updater.Tasks;

internal interface IProgressTask : IComponentTask
{
    ProgressType Type { get; }

    public ITaskProgressReporter ProgressReporter { get; }

    long Size { get; }
}