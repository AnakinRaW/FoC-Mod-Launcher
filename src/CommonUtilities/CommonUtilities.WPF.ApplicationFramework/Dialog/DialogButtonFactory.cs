using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Dialog;

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