using System;
using System.Windows.Input;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.Input;

public class DelegateCommand : IDelegateCommand
{
    public event EventHandler CanExecuteChanged
    {
        add
        {
            CommandManager.RequerySuggested += value;
            _canExecuteChanged += value;
        }
        remove
        {
            CommandManager.RequerySuggested -= value;
            _canExecuteChanged -= value;
        }
    }

    private readonly Action _execute;
    private readonly Func<bool> _canExecute;
    private EventHandler? _canExecuteChanged;

    public ICommand Command => this;

    public DelegateCommand(Action execute)
        : this(execute, null)
    {
    }

    public DelegateCommand(Action execute, Func<bool>? canExecute)
    {
        Requires.NotNull((object)execute, nameof(execute));
        _execute = execute;
        _canExecute = canExecute ?? (() => true);
    }
    
    public bool CanExecute()
    {
        return _canExecute();
    }

    public void Execute()
    {
        _execute();
    }

    public void RaiseCanExecuteChanged()
    {
        _canExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    bool ICommand.CanExecute(object parameter)
    {
        return CanExecute();
    }

    void ICommand.Execute(object parameter)
    {
        Execute();
    }
}