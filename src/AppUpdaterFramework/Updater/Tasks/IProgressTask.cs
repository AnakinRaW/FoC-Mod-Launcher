using AnakinRaW.AppUpdaterFramework.Updater.Progress;

namespace AnakinRaW.AppUpdaterFramework.Updater.Tasks;

internal interface IProgressTask : IComponentTask
{
    ProgressType Type { get; }

    public ITaskProgressReporter ProgressReporter { get; }

    long Size { get; }
}