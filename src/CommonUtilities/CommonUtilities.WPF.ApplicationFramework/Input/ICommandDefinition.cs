using System.Windows.Input;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;

public interface ICommandDefinition
{
    string Text { get; }

    string? Tooltip { get; }

    ImageKey Image { get; }

    ICommand Command { get; }
}