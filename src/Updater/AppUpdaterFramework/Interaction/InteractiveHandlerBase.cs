using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnakinRaW.AppUpdaterFramework.Interaction;

internal abstract class InteractiveHandlerBase
{
    protected readonly ILogger? Logger;
    protected IInteractionHandler InteractionHandler { get; }

    protected InteractiveHandlerBase(IServiceProvider serviceProvider)
    {
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        InteractionHandler = serviceProvider.GetRequiredService<IInteractionHandler>();
    }

    protected void HandleError(string message)
    {
        InteractionHandler.HandleError(message);
    }
}