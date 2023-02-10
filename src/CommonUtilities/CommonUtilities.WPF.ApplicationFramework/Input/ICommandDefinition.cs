using System.Threading.Tasks;
using System.Windows.Input;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using AnakinRaW.CommonUtilities.Wpf.Input;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;

public interface ICommandDefinition
{
    string Text { get; }

    string? Tooltip { get; }

    ImageKey Image { get; }

    ICommand Command { get; }
}

public interface ICommandHandler<in T> : ICommandHandler
{
    new IDelegateCommand<T> Command { get; }

    void Handle(T parameter);

    Task HandleAsync(T parameter);
}