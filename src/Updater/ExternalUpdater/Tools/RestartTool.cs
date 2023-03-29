using System;
using System.Threading.Tasks;
using AnakinRaW.ExternalUpdater.Options;

namespace AnakinRaW.ExternalUpdater.Tools;

internal sealed class RestartTool : ProcessTool<RestartOptions>
{
    public RestartTool(RestartOptions options, IServiceProvider serviceProvider) : base(options, serviceProvider)
    {
    }

    public override async Task<ExternalUpdaterResult> Run()
    {
        await WaitForProcessExitAsync();
        StartProcess(ExternalUpdaterResult.Restarted);
        return ExternalUpdaterResult.Restarted;
    }
}