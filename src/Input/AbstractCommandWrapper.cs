using System;
using System.Diagnostics;
using System.Windows.Input;

namespace FocLauncher.Input
{
    /*This class is doing some magic since the CanExecuteChanged in the base Command was not fired correctly*/

    /// <inheritdoc />
    /// <summary>
    /// Abstract command wrapper that triggers the <see cref="ICommand.CanExecuteChanged" /> correctly
    /// </summary>
    /// <seealso cref="ICommand" />
    public abstract class AbstractCommandWrapper : ICommand
    {

        protected AbstractCommandWrapper()
        {

        }

        protected AbstractCommandWrapper(Action executeAction, Func<bool> cantExecuteFunc)
        {
            if (executeAction == null)
                throw new ArgumentNullException(nameof(executeAction));
            if (cantExecuteFunc == null)
                throw new ArgumentNullException(nameof(cantExecuteFunc));
            WrappedCommand = new Command(executeAction, cantExecuteFunc);
        }

        protected AbstractCommandWrapper(ICommand wrappedCommand)
        {
            WrappedCommand = wrappedCommand ?? throw new ArgumentNullException(nameof(wrappedCommand));
        }

        public ICommand WrappedCommand { get; protected internal set; }

        public bool CanExecute(object parameter)
        {
            if (WrappedCommand == null)
                return false;
            return WrappedCommand.CanExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        [DebuggerHidden]
        public void Execute(object parameter)
        {
            WrappedCommand?.Execute(parameter);
        }
    }
}
