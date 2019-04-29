using System;
using System.Threading.Tasks;

namespace FocLauncher.Core.Input
{
    /// <inheritdoc />
    /// <summary>
    ///     An <see cref="ICommand" /> whose delegates do not take any parameters for <see cref="Execute" /> and
    ///     <see cref="CanExecute" />.
    /// </summary>
    /// <see cref="N:ModernApplicationFramework.CommandBase" />
    /// <see cref="T:ModernApplicationFramework.Input.Command.Command" />
    public class Command : CommandBase
    {
        /// <inheritdoc />
        /// <summary>
        ///     Creates a new instance of <see cref="T:ModernApplicationFramework.Input.Command.Command" /> with the
        ///     <see cref="T:System.Action" /> to invoke on execution.
        /// </summary>
        /// <param name="executeMethod">
        ///     The <see cref="T:System.Action" /> to invoke when
        ///     <see cref="M:System.Windows.Input.ICommand.Execute(System.Object)" /> is called.
        /// </param>
        public Command(Action executeMethod) : this(executeMethod, () => true)
        {
        }

        public Command(Action<object> executeMethod) : this(executeMethod, o => true)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="Command" /> with the <see cref="Action" /> to invoke on execution
        ///     and a <see langword="Func" /> to query for determining if the command can execute.
        /// </summary>
        /// <param name="executeMethod">The <see cref="Action" /> to invoke when <see cref="ICommand.Execute" /> is called.</param>
        /// <param name="canExecuteMethod">
        ///     The <see cref="Func{TResult}" /> to invoke when <see cref="ICommand.CanExecute" /> is
        ///     called
        /// </param>
        public Command(Action executeMethod, Func<bool> canExecuteMethod)
            : base(o => executeMethod(), o => canExecuteMethod())
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException(nameof(executeMethod));
        }

        private Command(Func<Task> executeMethod) : this(executeMethod, () => true)
        {
        }

        private Command(Func<Task> executeMethod, Func<bool> canExecuteMethod)
            : base(o => executeMethod(), o => canExecuteMethod())
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException(nameof(executeMethod));
        }

        private Command(Func<object, Task> executeMethod) : this(executeMethod, o => true)
        {
        }

        private Command(Func<object, Task> executeMethod, Func<object, bool> canExecuteMethod)
            : base(executeMethod, canExecuteMethod)
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException(nameof(executeMethod));
        }

        public Command(Action<object> executeMethod, Func<object, bool> canExecuteMethod)
            : base(executeMethod, canExecuteMethod)
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException(nameof(executeMethod));
        }

        public static Command FromAsyncHandler(Func<Task> executeMethod)
        {
            return new Command(executeMethod);
        }

        public static Command FromAsyncHandler(Func<Task> executeMethod, Func<bool> canExecuteMethod)
        {
            return new Command(executeMethod, canExecuteMethod);
        }

        public static Command FromAsyncHandler(Func<object, Task> executeMethod)
        {
            return new Command(executeMethod);
        }

        public static Command FromAsyncHandler(Func<object, Task> executeMethod, Func<object, bool> canExecuteMethod)
        {
            return new Command(executeMethod, canExecuteMethod);
        }


        /// <summary>
        ///     Determines if the command can be executed.
        /// </summary>
        /// <returns>Returns <see langword="true" /> if the command can execute,otherwise returns <see langword="false" />.</returns>
        public virtual bool CanExecute()
        {
            return CanExecute(null);
        }

        /// <summary>
        ///     Executes the command.
        /// </summary>
        public virtual async Task Execute()
        {
            await Execute(null);
        }
    }
}
