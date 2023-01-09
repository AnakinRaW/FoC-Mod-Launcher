using System.Windows.Input;

namespace AnakinRaW.CommonUtilities.Wpf.Input;

public interface IDelegateCommand : ICommand
{
    void RaiseCanExecuteChanged();

    ICommand Command { get; }
}