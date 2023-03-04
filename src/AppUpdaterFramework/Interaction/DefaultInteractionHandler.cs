using System;
using System.IO.Abstractions;

namespace AnakinRaW.AppUpdaterFramework.Interaction;

internal class DefaultInteractionHandler : IInteractionHandler
{
    public DefaultInteractionHandler(IServiceProvider serviceProvider)
    {
    }

    public InteractionStatus HandleLockedFile(IFileInfo file, SupportedInteractions supportedStates)
    {
        return InteractionStatus.Abort;
    }
}