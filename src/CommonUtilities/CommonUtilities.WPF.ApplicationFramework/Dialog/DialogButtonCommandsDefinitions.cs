using System.Windows.Input;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using AnakinRaW.CommonUtilities.Wpf.Input;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;

public static class DialogButtonCommandsDefinitions
{
    internal static readonly ICommand CloseDialogCommand =
        new DelegateCommand<IDialogViewModel>(vm => vm?.CloseDialog(), vm => vm != null);

    public static ICommandDefinition OkCommandDefinition { get; } = Create("OK");

    public static ICommandDefinition CancelCommandDefinition { get; } = Create("Cancel");

    public static ICommandDefinition RetryCommandDefinition { get; } = Create("Retry");

    public static ICommandDefinition Create(string name)
    {
        return new DialogCommand(name, default, CloseDialogCommand);
    }

    public static ICommandDefinition Create(string name, ImageKey image)
    {
        return new DialogCommand(name, image, CloseDialogCommand);
    }

    private class DialogCommand : CommandDefinition
    {
        public override string Text { get; }
        public override ICommand Command { get; }

        public override ImageKey Image { get; }

        public DialogCommand(string text, ImageKey image, ICommand command)
        {
            Text = text;
            Command = command;
            Image = image;
        }
    }
}