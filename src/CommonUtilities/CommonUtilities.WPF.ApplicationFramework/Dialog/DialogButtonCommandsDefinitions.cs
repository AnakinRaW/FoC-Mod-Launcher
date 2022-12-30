using System.Windows.Input;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;
using Sklavenwalker.CommonUtilities.Wpf.Input;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;

public static class DialogButtonCommandsDefinitions
{
    internal static readonly ICommand CloseDialogCommand =
        new DelegateCommand<IDialogViewModel>(vm => vm?.CloseDialog(), vm => vm != null);

    public static ICommandDefinition OkCommandDefinition { get; } = new DialogCommand("OK", CloseDialogCommand);

    public static ICommandDefinition CancelCommandDefinition { get; } = new DialogCommand("Cancel", CloseDialogCommand);

    public static ICommandDefinition RetryCommandDefinition { get; } = new DialogCommand("Retry", CloseDialogCommand);

    private class DialogCommand : CommandDefinition
    {
        public override string Text { get; }
        public override ICommand Command { get; }

        public DialogCommand(string text, ICommand command)
        {
            Text = text;
            Command = command;
        }
    }
}