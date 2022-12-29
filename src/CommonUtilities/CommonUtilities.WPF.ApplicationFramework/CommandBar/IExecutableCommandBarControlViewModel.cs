using System.Windows.Input;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar;

public interface IExecutableCommandBarControlViewModel : ICommandBarControlViewModel
{
    string? Tooltip { get; set; }

    ICommand Command { get; }
}