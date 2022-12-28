namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;

public interface IButtonViewModel
{
    string Id { get; }

    ICommandDefinition CommandDefinition { get; }

    bool IsDefault { get; }

    bool IsCancel { get; }

    bool Themed { get; }
}