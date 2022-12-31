using System.Windows.Input;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.CommandBar;

internal interface IExecutableCommandBarControlViewModel : ICommandBarControlViewModel
{
    string? Tooltip { get; set; }

    ICommand Command { get; }
}