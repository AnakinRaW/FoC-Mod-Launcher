using System;
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

    void ICommandHandler.Handle()
    {
        throw new NotSupportedException();
    }

    Task ICommandHandler.HandleAsync()
    {
        throw new NotSupportedException();
    }
}


public abstract class CommandHandlerBase : ICommandHandler
{
    public IDelegateCommand Command { get; }

    protected CommandHandlerBase()
    {
        Command = new DelegateCommand(Handle, CanHandle);
    }

    public abstract void Handle();

    public virtual Task HandleAsync()
    {
        return Task.Run(Handle);
    }

    protected virtual bool CanHandle()
    {
        return true;
    }

    IDelegateCommand ICommandHandler.Command => Command;
}