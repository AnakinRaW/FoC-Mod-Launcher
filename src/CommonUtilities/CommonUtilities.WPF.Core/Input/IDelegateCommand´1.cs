namespace Sklavenwalker.CommonUtilities.Wpf.Input;

public interface IDelegateCommand<in T> : IDelegateCommand
{
    bool CanExecute(T? parameter);

    void Execute(T parameter);
}