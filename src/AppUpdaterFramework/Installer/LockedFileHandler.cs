using System;
using System.IO.Abstractions;
using AnakinRaW.AppUpaterFramework.Interaction;
using AnakinRaW.AppUpaterFramework.Metadata.Component;

namespace AnakinRaW.AppUpaterFramework.Installer;

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