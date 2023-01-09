using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog.Buttons;

public interface IButtonViewModel
{
    string Id { get; }

    ICommandDefinition CommandDefinition { get; }

    bool IsDefault { get; }

    bool IsCancel { get; }

    bool Themed { get; }
}