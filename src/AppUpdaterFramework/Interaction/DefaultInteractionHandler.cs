using System;
using System.IO.Abstractions;

namespace AnakinRaW.AppUpaterFramework.Interaction;

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