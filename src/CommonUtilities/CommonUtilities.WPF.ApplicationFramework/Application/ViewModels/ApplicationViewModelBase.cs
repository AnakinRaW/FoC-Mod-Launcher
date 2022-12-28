using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.StatusBar;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

public abstract partial class ApplicationViewModelBase : MainWindowViewModel, IApplicationViewModel
{
    protected IServiceProvider ServiceProvider { get; }
    protected ILogger? Logger { get; }

    private bool _isDisposed;

    [ObservableProperty] private IViewModel? _currentViewModel;

    protected ApplicationViewModelBase(IStatusBarViewModel statusBarViewModel, IServiceProvider serviceProvider) : base(statusBarViewModel)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        ServiceProvider = serviceProvider;
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public abstract Task InitializeAsync();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void OnDispose()
    {
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;
        if (disposing)
            OnDispose();
        _isDisposed = true;
    }
}