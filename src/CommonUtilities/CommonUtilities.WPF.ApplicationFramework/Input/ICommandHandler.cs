using AnakinRaW.CommonUtilities.Wpf.Input;
using System.Threading.Tasks;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;

public interface ICommandHandler
{
    IDelegateCommand Command { get; }

    void Handle();

    Task HandleAsync();
}