using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FocLauncher.ViewModels;

public abstract class ViewModelBase : ObservableObject, ILauncherViewModel, IDisposable
{
    protected readonly IServiceProvider ServiceProvider;

    private bool _isDisposed;

    protected ViewModelBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    ~ViewModelBase()
    {
        Dispose(false);
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public void Dispose() => Dispose(true);

    protected virtual void DisposeImpl()
    {
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;
        if (disposing)
            DisposeImpl();
        _isDisposed = true;
    }
}