using System.IO.Abstractions;

namespace AnakinRaW.AppUpaterFramework.Interaction;

public interface IInteractionHandler
{
    InteractionStatus HandleLockedFile(IFileInfo file, SupportedInteractions supportedStates);
}

internal class NullInteractionHandler : IInteractionHandler
{
    public InteractionStatus HandleLockedFile(IFileInfo file, SupportedInteractions supportedStates)
    {
        return InteractionStatus.Abort;
    }
}