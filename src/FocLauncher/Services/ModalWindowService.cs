using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using FocLauncher.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Validation;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Interop;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FocLauncher.Services;

interface ILauncherModalViewModel : IModalWindowViewModel, ILauncherViewModel
{
}




internal partial class UpdateWindowViewModel : ModalWindowViewModel, ILauncherModalViewModel, ILoadingViewModel
{
    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string? _loadingText;

    public ICommand ClickCommand => new RelayCommand(() => throw new Exception());

    public UpdateWindowViewModel()
    {
        HasMaximizeButton = false;
        HasMinimizeButton = false;
    }

    public Task InitializeAsync()
    {
        return Task.Run(async () =>
        {
            //await Task.Delay(3000);
            throw new Exception();
        });
    }

    public void OnClosing(CancelEventArgs e)
    {
    }
}

internal interface IModalWindowService
{
    Task ShowModal(ILauncherModalViewModel viewModel);
}

internal interface IModalWindowFactory
{
    ModalWindow Create(ILauncherModalViewModel viewModel);
}

internal class ModalWindowFactory : IModalWindowFactory
{
    private readonly IWindowService _windowService;

    public ModalWindowFactory(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _windowService = serviceProvider.GetRequiredService<IWindowService>();
    }

    public ModalWindow Create(ILauncherModalViewModel viewModel)
    {
        return Application.Current.Dispatcher.Invoke(() =>
        {
            _windowService.ShowWindow();
            var window = new ModalWindow(viewModel);
            window.Content = viewModel;
            _windowService.SetOwner(window);
            return window;
        });
    }
}

internal class ModalWindowService : IModalWindowService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IModalWindowFactory _windowFactory;
    private readonly IWindowService _windowService;
    private readonly ILogger? _logger;
    private readonly IQueuedDialogService _queuedDialogService;

    public ModalWindowService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _windowFactory = serviceProvider.GetRequiredService<IModalWindowFactory>();
        _windowService = serviceProvider.GetRequiredService<IWindowService>();
        _queuedDialogService = serviceProvider.GetRequiredService<IQueuedDialogService>();
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public async Task ShowModal(ILauncherModalViewModel viewModel)
    {
        var task = new TaskCompletionSource<bool>();

        var window = _windowFactory.Create(viewModel);
        _logger?.LogTrace($"Showing window: {viewModel}");
        window.Closing += OnDialogClosing(window, viewModel, task);
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
        {
            _windowService.DisableOwner(window);
            window.ShowModal();
        });

        try
        {
            await viewModel.InitializeAsync();
        }
        catch (Exception e)
        {
            const string header = "error.";
            await _queuedDialogService.ShowDialog(new ErrorMessageDialogViewModel(header, "message", _serviceProvider));
        }
        

        await task.Task;
    }

    private CancelEventHandler OnDialogClosing(Window window, ILauncherModalViewModel viewModel, TaskCompletionSource<bool> task)
    {
        return (_, e) =>
        {
            viewModel.OnClosing(e);
            if (e.Cancel)
                return;
            _windowService.EnableOwner(window);
            task.SetResult(true);
        };
    }
}

internal class WindowService : IWindowService
{
    private Window _mainWindow;
    private readonly object _syncObject = new();
    
    public void SetMainWindow(Window window)
    {
        if (_mainWindow is not null)
            throw new InvalidOperationException("Main Window already set.");
        lock (_syncObject)
        {
            if (_mainWindow is not null)
                throw new InvalidOperationException("Main Window already set.");
            _mainWindow = window;
        }
    }

    public void ShowWindow()
    {
        ValidateMainWindow();
        Application.Current.Dispatcher.Invoke(_mainWindow.Show);
    }

    public void SetOwner(Window window)
    {
        ValidateMainWindow();
        if (window is WindowBase windowBase)
        {
            var mwh = new WindowInteropHelper(_mainWindow).Handle;
            windowBase.ChangeOwnerForActivate(mwh);
            windowBase.ChangeOwner(mwh);
        }
        else window.Owner = _mainWindow;
    }

    public void DisableOwner(Window window)
    {
        ValidateMainWindow();
        if (window.Owner == null)
            return;
        window.Owner.IsEnabled = false;
    }

    public void EnableOwner(Window window)
    {
        ValidateMainWindow();
        if (window.Owner == null)
            return;
        window.Owner.IsEnabled = true;
        if (window.Owner.IsActive)
            return;
        window.Owner.Activate();
    }

    private void ValidateMainWindow()
    {
        if (_mainWindow is null)
            throw new InvalidOperationException("No main window set.");
    }
}