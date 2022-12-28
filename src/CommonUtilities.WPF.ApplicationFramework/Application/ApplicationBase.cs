using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Controls;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework;

public abstract class ApplicationBase : Application
{
    private bool _windowShown;
    private readonly object _syncObject = new();

    protected IServiceProvider ServiceProvider { get; }
    protected ILogger? Logger { get; }

    protected ApplicationBase(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        ServiceProvider = serviceProvider;
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        PreWindowInitialize();
        var applicationViewModel = CreateApplicationViewModel();
        var window = InitializeWindow(applicationViewModel);
        ShowWindow(window);
        PostWindowInitialize(applicationViewModel);
    }
    
    protected abstract IApplicationViewModel CreateApplicationViewModel();

    private void PreWindowInitialize()
    {
        InitializeResources();
        InitializeServices();
    }

    private void PostWindowInitialize(IApplicationViewModel viewModel)
    {
        viewModel.InitializeAsync().Wait();
    }

    protected virtual void InitializeResources()
    {
    }

    protected virtual void InitializeServices()
    {
    }

    protected abstract ApplicationMainWindow CreateMainWindow(IMainWindowViewModel viewModel);

    private Window InitializeWindow(IMainWindowViewModel viewModel)
    {
        var window = CreateMainWindow(viewModel);
        MainWindow = window;
        ServiceProvider.GetRequiredService<IWindowService>().SetMainWindow(window);
        return window;
    }

    private void ShowWindow(Window window)
    {
        if (_windowShown)
            return;
        lock (_syncObject)
        {
            if (_windowShown)
                return;
            _windowShown = true;
        }
        Logger?.LogTrace("Showing the window.");
        Dispatcher.Invoke(window.Show);
    }
}