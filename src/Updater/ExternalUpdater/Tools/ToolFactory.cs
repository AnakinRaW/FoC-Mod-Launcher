using System;
using AnakinRaW.ExternalUpdater.CLI.Arguments;

namespace AnakinRaW.ExternalUpdater.Tools;

internal class ToolFactory
{
    public ITool Create(ExternalUpdaterArguments arguments, IServiceProvider serviceProvider)
    {
        return arguments switch
        {
            UpdateArguments updateArguments => new UpdateTool(updateArguments, serviceProvider),
            RestartArguments restartArguments => new RestartTool(restartArguments, serviceProvider),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}