using AnakinRaW.AppUpaterFramework.Metadata.Component;
using AnakinRaW.AppUpaterFramework.Metadata.Update;
using AnakinRaW.CommonUtilities.TaskPipeline.Tasks;
using System.Threading;
using System;
using AnakinRaW.AppUpaterFramework.Updater.Progress;

namespace AnakinRaW.AppUpaterFramework.Updater.Tasks;

internal class InstallTask : RunnerTask, IProgressTask
{
    public IProductComponent Component { get; }

    internal InstallResult Result { get; set; } = InstallResult.Success;

    public long Weight { get; }

    public long Size { get; }

    public ProgressType Type { get; }

    public InstallTask(IInstallableComponent installable, UpdateAction action, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Component = installable;
    }

    protected override void RunCore(CancellationToken token)
    {
    }

    public ITaskProgressReporter ProgressReporter { get; }
}