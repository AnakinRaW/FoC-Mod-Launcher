using System;
using System.Threading;
using AnakinRaW.CommonUtilities.TaskPipeline;

namespace AnakinRaW.AppUpdaterFramework.Updater;

internal class UpdateCleanJob : JobBase
{
    public UpdateCleanJob(IServiceProvider serviceProvider)
    {
    }

    protected override bool PlanCore()
    {
        return true;
    }

    protected override void RunCore(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
    }
}