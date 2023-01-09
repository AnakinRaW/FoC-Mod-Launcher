using System.Windows.Input;
using AnakinRaW.CommonUtilities.Wpf.Imaging;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;

public interface ICommandDefinition
{
    string Text { get; }

    string? Tooltip { get; }

    ImageKey Image { get; }

    ICommand Command { get; }
}