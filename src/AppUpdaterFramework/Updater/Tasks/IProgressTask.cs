using AnakinRaW.AppUpaterFramework.Updater.Progress;

namespace AnakinRaW.AppUpaterFramework.Updater.Tasks;

internal interface IProgressTask : IComponentTask
{
    public ITaskProgressReporter ProgressReporter { get; }

    //long Weight { get; }

    //long Size { get; }

    //ProgressType Type { get; }
}