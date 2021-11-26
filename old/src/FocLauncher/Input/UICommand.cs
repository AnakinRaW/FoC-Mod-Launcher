using System;
using System.Windows.Input;

namespace FocLauncher.Input
{
    public class UICommand : AbstractCommandWrapper
    {
        public UICommand(Action<object> executeAction, Func<object, bool> cantExecuteFunc) : base(executeAction, cantExecuteFunc)
        {
        }

        public UICommand(Action executeAction, Func<bool> cantExecuteFunc) : base(executeAction, cantExecuteFunc)
        {
        }

        public UICommand(ICommand wrappedCommand) : base(wrappedCommand)
        {
        }
    }
}
