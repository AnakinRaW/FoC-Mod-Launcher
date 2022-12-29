using System.Windows.Input;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Input;

public abstract class CommandDefinition : ICommandDefinition
{
    public virtual ImageKey Image => default;

    public virtual string Tooltip => string.Empty;

    public abstract string Text { get; }

    public abstract ICommand Command { get; }
}