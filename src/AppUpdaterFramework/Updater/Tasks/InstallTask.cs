using AnakinRaW.AppUpaterFramework.Metadata.Component;
using AnakinRaW.AppUpaterFramework.Metadata.Update;
using AnakinRaW.CommonUtilities.TaskPipeline.Tasks;
using System.Threading;
using System;
using System.Threading.Tasks;
using AnakinRaW.AppUpaterFramework.Updater.Progress;

namespace AnakinRaW.AppUpaterFramework.Updater.Tasks;

internal class InstallTask : RunnerTask, IProgressTask
{
    public IProductComponent Component { get; }
    
    private UpdateAction Action { get; }

    internal InstallResult Result { get; set; } = InstallResult.Success;

    public ITaskProgressReporter ProgressReporter { get; }

    public ProgressType Type => ProgressType.Install;

    public long Size => Component is IInstallableComponent installable ? installable.InstallationSize.Total : 0;

    public InstallTask(IInstallableComponent installable, UpdateAction action, ITaskProgressReporter progressReporter, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Component = installable;
        Action = action;
        ProgressReporter = progressReporter;
    }

    protected override void RunCore(CancellationToken token)
    {
        for (int i = 0; i < 100; i++)
        {
            ProgressReporter.Report(this, (double)i / 100, new ProgressInfo());
            Task.Delay(50, token).Wait(token);
        }
    }
}