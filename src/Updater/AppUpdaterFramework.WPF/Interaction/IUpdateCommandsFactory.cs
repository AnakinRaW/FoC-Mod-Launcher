using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;

namespace AnakinRaW.AppUpdaterFramework.Interaction;

public interface IUpdateCommandsFactory
{
    ICommandDefinition CreateRestart();

    ICommandDefinition CreateElevate();
}