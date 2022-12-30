using System.Windows.Input;

namespace Sklavenwalker.CommonUtilities.Wpf.Input;

public interface IDelegateCommand : ICommand
{
    void RaiseCanExecuteChanged();

    ICommand Command { get; }
}