using System;
using System.IO.Abstractions;
using AnakinRaW.AppUpdaterFramework.Interaction;
using AnakinRaW.AppUpdaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpdaterFramework.Installer;

internal class LockedFileHandler : ILockedFileHandler
{
    public LockedFileHandler(IServiceProvider serviceProvider)
    {
    }

    public ILockedFileHandler.Result Handle(IInstallableComponent component, IFileInfo file)
    {

        var supportedStates = new SupportedInteractions(
            new SupportedInteractionState(InteractionStatus.Retry, ""));

        //if (!supportedStates.Contains(interactionStatus))
        //    throw new InvalidOperationException("Retrieved interaction status was not expected.");


        return ILockedFileHandler.Result.Locked;
    }
}