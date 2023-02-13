using System;
using System.Threading;
using AnakinRaW.AppUpaterFramework.Metadata.Component;
using AnakinRaW.CommonUtilities.TaskPipeline.Tasks;

namespace AnakinRaW.AppUpaterFramework.Updater.Tasks;

internal class DownloadTask : SynchronizedTask, IComponentTask
{
    public IProductComponent Component { get; }

    public DownloadTask(IInstallableComponent installable, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Component = installable;
    }

    protected override void SynchronizedInvoke(CancellationToken token)
    {
    }
}