using AnakinRaW.AppUpaterFramework.Metadata.Component;
using AnakinRaW.AppUpaterFramework.Metadata.Update;
using AnakinRaW.CommonUtilities.TaskPipeline.Tasks;
using System.Threading;
using System;

namespace AnakinRaW.AppUpaterFramework.Updater.Tasks;

internal class InstallTask : RunnerTask, IComponentTask
{
    public IProductComponent Component { get; }

    internal InstallResult Result { get; set; } = InstallResult.Success;

    public InstallTask(IInstallableComponent installable, UpdateAction action, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Component = installable;
    }

    protected override void RunCore(CancellationToken token)
    {
    }
}