using System.IO.Abstractions;

namespace AnakinRaW.AppUpdaterFramework.Interaction;

public interface IInteractionHandler
{
    InteractionStatus HandleLockedFile(IFileInfo file, SupportedInteractions supportedStates);
}