using System;
using System.Threading.Tasks;
using AnakinRaW.ExternalUpdater.CLI;
using AnakinRaW.ExternalUpdater.CLI.Arguments;

namespace AnakinRaW.ExternalUpdater.Tools;

internal class RestartTool : ProcessTool<RestartArguments>
{
    public RestartTool(RestartArguments arguments, IServiceProvider serviceProvider) : base(arguments, serviceProvider)
    {
    }

    public override async Task<ExternalUpdaterResult> Run()
    {
        await WaitForProcessExitAsync();
        StartProcess(ExternalUpdaterResult.NoUpdate);
        return ExternalUpdaterResult.NoUpdate;
    }
}