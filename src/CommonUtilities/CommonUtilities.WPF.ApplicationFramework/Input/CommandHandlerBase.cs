using System.Threading.Tasks;
using AnakinRaW.CommonUtilities.Wpf.Input;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;

public abstract class CommandHandlerBase<T> : ICommandHandler<T>
{
    public IDelegateCommand<T> Command { get; }

    protected CommandHandlerBase()
    {
        Command = new DelegateCommand<T>(Handle, CanHandle);
    }

    public abstract void Handle(T parameter);

    public virtual Task HandleAsync(T parameter)
    {
        return Task.Run(() => Handle(parameter));
    }

    public virtual bool CanHandle(T? parameter)
    {
        return true;
    }

    IDelegateCommand ICommandHandler.Command => Command;
}