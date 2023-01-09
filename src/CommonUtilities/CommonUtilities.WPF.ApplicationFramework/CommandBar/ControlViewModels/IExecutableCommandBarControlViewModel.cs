using System.Windows.Input;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.CommandBar;

internal interface IExecutableCommandBarControlViewModel : ICommandBarControlViewModel
{
    string? Tooltip { get; set; }

    ICommand Command { get; }
}