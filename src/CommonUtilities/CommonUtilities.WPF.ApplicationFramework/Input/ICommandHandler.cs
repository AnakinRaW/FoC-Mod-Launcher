using AnakinRaW.CommonUtilities.Wpf.Input;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;

public interface ICommandHandler
{
    IDelegateCommand Command { get; }
}