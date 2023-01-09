using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Models;

public interface ICommandItemDefinition
{
    ICommandDefinition CommandDefinition { get; }
}