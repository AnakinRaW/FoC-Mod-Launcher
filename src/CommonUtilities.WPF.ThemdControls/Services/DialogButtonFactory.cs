using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.Services;

public class DialogButtonFactory : IDialogButtonFactory
{
    public bool ThemeButtons { get; }

    public DialogButtonFactory(bool themedFactory)
    {
        ThemeButtons = themedFactory;
    }

    public IButtonViewModel CreateOk(bool isDefault)
    {
        return CreateButton(DefaultDialogButtonIdentifiers.Ok, DialogButtonCommandsDefinitions.OkCommandDefinition, isDefault, false);
    }

    public IButtonViewModel CreateCancel(bool isDefault)
    {
        return CreateButton(DefaultDialogButtonIdentifiers.Cancel, DialogButtonCommandsDefinitions.CancelCommandDefinition, isDefault, true);
    }

    public IButtonViewModel CreateRetry(bool isDefault)
    {
        return CreateButton(DefaultDialogButtonIdentifiers.Retry, DialogButtonCommandsDefinitions.RetryCommandDefinition, isDefault, false);
    }

    protected virtual IButtonViewModel CreateButton(string id, ICommandDefinition commandDefinition, bool isDefault, bool cancel)
    {
        return new ButtonViewModel(id, commandDefinition)
        {
            IsCancel = cancel,
            IsDefault = isDefault,
            Themed = ThemeButtons
        };
    }
}

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