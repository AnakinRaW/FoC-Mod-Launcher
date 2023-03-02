using System.IO.Abstractions;

namespace AnakinRaW.AppUpaterFramework.Interaction;

public interface IInteractionHandler
{
    InteractionStatus HandleLockedFile(IFileInfo file);
}