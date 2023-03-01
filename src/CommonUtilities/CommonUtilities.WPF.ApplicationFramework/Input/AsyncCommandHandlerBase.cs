using System.Threading.Tasks;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;

public abstract class AsyncCommandHandlerBase<T> : CommandHandlerBase<T>
{
    public override void Handle(T parameter) => Task.Run(() => HandleAsync(parameter));

    public abstract override Task HandleAsync(T parameter);
}

public abstract class AsyncCommandHandlerBase : CommandHandlerBase
{
    public override void Handle() => Task.Run(HandleAsync);

    public abstract override Task HandleAsync();
}