using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;

public static class DialogButtonCommandsDefinitions
{
    private static readonly ICommand CloseDialogCommand =
        new RelayCommand<IDialogViewModel>(vm => vm?.CloseDialog(), vm => vm != null);

    public static ICommandDefinition OkCommandDefinition { get; } = new CommandDefinition("OK")
    {
        Command = CloseDialogCommand
    };

    public static ICommandDefinition CancelCommandDefinition { get; } = new CommandDefinition("Cancel")
    {
        Command = CloseDialogCommand
    };

    public static ICommandDefinition RetryCommandDefinition { get; } = new CommandDefinition("Retry")
    {
        Command = CloseDialogCommand
    };
}