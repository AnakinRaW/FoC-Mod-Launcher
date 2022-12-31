using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar.Models;

public interface ICommandItemDefinition
{
    ICommandDefinition CommandDefinition { get; }
}