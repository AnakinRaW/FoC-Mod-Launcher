using System;
using System.Windows.Input;
using Validation;

namespace AnakinRaW.CommonUtilities.Wpf.Input;

public class DelegateCommand<T> : IDelegateCommand<T>
{
    private readonly Action<T> _execute;
    private readonly Predicate<T?>? _canExecute;
    private EventHandler? _canExecuteChanged;

    public DelegateCommand(Action<T?> execute)
        : this(execute, null)
    {
    }

    public DelegateCommand(Action<T> execute, Predicate<T?>? canExecute)
    {
        Requires.NotNull((object)execute, nameof(execute));
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
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

    public void RaiseCanExecuteChanged()
    {
        _canExecuteChanged?.Invoke(this, null!);
    }

    public bool CanExecute(T? parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    public void Execute(T parameter)
    {
        _execute(parameter);
    }

    public ICommand Command => this;

    bool ICommand.CanExecute(object? parameter)
    {
        if (_canExecute == null)
            return true;
        if (parameter == null)
            return CanExecute(default!);
        return parameter is T parameter1 ? CanExecute(parameter1) : throw new ArgumentException($"{nameof(parameter)} is not of type {typeof(T)}");
    }

    void ICommand.Execute(object? parameter)
    {
        if (parameter == null)
        {
            Execute(default!);
        }
        else
        {
            if (parameter is not T tType)
                throw new ArgumentException($"{nameof(parameter)} is not of type {typeof(T)}");
            Execute(tType);
        }
    }
}