using System;
using System.Windows.Input;

namespace FocLauncher.Core.Input
{
    public class UICommand : AbstractCommandWrapper
    {
        public UICommand(Action executeAction, Func<bool> cantExecuteFunc) : base(executeAction, cantExecuteFunc)
        {
        }

        public UICommand(ICommand wrappedCommand) : base(wrappedCommand)
        {
        }
    }
}
