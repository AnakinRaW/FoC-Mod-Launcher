using System;
using AnakinRaW.ExternalUpdater.Options;

namespace AnakinRaW.ExternalUpdater.Tools;

internal class ToolFactory
{
    public ITool Create(ExternalUpdaterOptions options, IServiceProvider serviceProvider)
    {
        return options switch
        {
            UpdateOptions updateArguments => new UpdateTool(updateArguments, serviceProvider),
            RestartOptions restartArguments => new RestartTool(restartArguments, serviceProvider),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}