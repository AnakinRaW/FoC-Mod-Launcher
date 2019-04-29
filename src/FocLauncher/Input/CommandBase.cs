using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FocLauncher.Core.Input
{
    /// <inheritdoc />
    /// <summary>
    /// An <see cref="ICommand" /> whose delegates can be attached for <see cref="Execute(Object)" /> and <see cref="CanExecute(Object)" />.
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        protected readonly Func<object, bool> CanExecuteMethod;
        protected readonly Func<object, Task> ExecuteMethod;
        private List<WeakReference> _canExecuteChangedHandlers;

        protected CommandBase(Action<object> executeMethod, Func<object, bool> canExecuteMethod)
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException(nameof(executeMethod));

            ExecuteMethod = args =>
            {
                executeMethod(args);
                return Task.Delay(0);
            };
            CanExecuteMethod = canExecuteMethod;
        }

        protected CommandBase(Func<object, Task> executeMethod, Func<object, bool> canExecuteMethod)
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException(nameof(executeMethod));

            ExecuteMethod = executeMethod;
            CanExecuteMethod = canExecuteMethod;
        }

        [DebuggerHidden]
        async void ICommand.Execute(object parameter)
        {
            await Execute(parameter);
        }

        bool ICommand.CanExecute(object parameter) => CanExecute(parameter);

        /// <summary>
        /// Occurs when changes occur that affect whether or not the command should execute.
        /// </summary>
        public virtual event EventHandler CanExecuteChanged
        {
            add => WeakEventHandlerManager.AddWeakReferenceHandler(ref _canExecuteChangedHandlers, value, 2);
            remove => WeakEventHandlerManager.RemoveWeakReferenceHandler(_canExecuteChangedHandlers, value);
        }

        /// <summary>
        /// Raises <see cref="CanExecuteChanged"/> so every command invoker
        /// can requery to check if the command can execute.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged();
        }

        /// <summary>
        /// Raises <see cref="ICommand.CanExecuteChanged"/> so every 
        /// command invoker can requery <see cref="ICommand.CanExecute"/>.
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
            WeakEventHandlerManager.CallWeakReferenceHandlers(this, _canExecuteChangedHandlers);
        }

        protected bool CanExecute(object parameter)
        {
            return CanExecuteMethod == null || CanExecuteMethod(parameter);
        }

#pragma warning disable IDE1006
        protected async Task Execute(object parameter)
#pragma warning restore IDE1006
        {
            await ExecuteMethod(parameter);
        }
    }
}
