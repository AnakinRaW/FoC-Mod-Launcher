using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using AnakinRaW.CommonUtilities.Wpf.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Validation;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;

internal class ModalWindowService : IModalWindowService
{
    private readonly IModalWindowFactory _windowFactory;
    private readonly IWindowService _windowService;
    private readonly ILogger? _logger;

    public ModalWindowService(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _windowFactory = serviceProvider.GetRequiredService<IModalWindowFactory>();
        _windowService = serviceProvider.GetRequiredService<IWindowService>();
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public async Task ShowModal(IModalWindowViewModel viewModel)
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

        if (viewModel is IViewModel initializingViewModel)
            await initializingViewModel.InitializeAsync();

        await task.Task;
    }

    private CancelEventHandler OnDialogClosing(Window window, IModalWindowViewModel viewModel, TaskCompletionSource<bool> task)
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