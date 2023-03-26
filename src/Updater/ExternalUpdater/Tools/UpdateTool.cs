using System;
using System.Threading.Tasks;
using AnakinRaW.ExternalUpdater.CLI;
using AnakinRaW.ExternalUpdater.CLI.Arguments;

namespace AnakinRaW.ExternalUpdater.Tools;

internal class UpdateTool : ToolBase<UpdateArguments>
{
    public UpdateTool(UpdateArguments arguments, IServiceProvider serviceProvider) : base(arguments, serviceProvider)
    {
    }

    public override async Task<ExternalUpdaterResult> Run()
    {
        return ExternalUpdaterResult.NoUpdate;
    }
}