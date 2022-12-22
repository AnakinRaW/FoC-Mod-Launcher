using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Validation;

namespace FocLauncher.Services;

internal class QueuedDialogService : IQueuedDialogService
{
    private readonly object _syncObject = new();
    private readonly ILogger? _logger;
    private readonly IDialogFactory _dialogFactory;
    private bool _isDialogOpen;

    private readonly Queue<(IDialogViewModel ViewModel, TaskCompletionSource<string?> Task)> _queuedDialogs = new();

    private bool ShouldShowNextDialog => !_isDialogOpen && _queuedDialogs.Count > 0;

    public QueuedDialogService(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _dialogFactory = serviceProvider.GetRequiredService<IDialogFactory>();
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public Task<string?> ShowDialog(IDialogViewModel viewModel)
    {
        var tcs = new TaskCompletionSource<string?>();
        QueueDialog(viewModel, tcs);
        ShowDialogAsync();
        return tcs.Task;
    }

    private void QueueDialog(IDialogViewModel viewModel, TaskCompletionSource<string?> tcs)
    {
        var dialogData = (viewModel, tcs);
        lock (_syncObject)
            _queuedDialogs.Enqueue(dialogData);
    }

    private Task ShowDialogAsync()
    {
        return Task.Run(() =>
        {
            if (!ShouldShowNextDialog)
                return;
            lock (_syncObject)
            {
                if (!ShouldShowNextDialog)
                    return;
                _isDialogOpen = true;
                ShowDialogAsync(CreateNextDialog());
            }
        });
    }

    private static void ShowDialogAsync(ModalWindow window)
    {
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, window.ShowModal);
    }

    private ModalWindow CreateNextDialog()
    {
        var (viewModel, task) = _queuedDialogs.Dequeue();
        var dialog = _dialogFactory.Create(viewModel);
        _logger?.LogTrace($"Showing window: {viewModel}");
        dialog.Closing += OnDialogClosing(dialog, viewModel, task);
        return dialog;
    }

    private CancelEventHandler OnDialogClosing(ModalWindow window, IDialogViewModel viewModel, TaskCompletionSource<string?> task)
    {
        return (_,_) =>
        {
            window.EnableOwner();
            task.SetResult(viewModel.ResultButton);
            _logger?.LogTrace($"Button selected: {viewModel.ResultButton}");
            lock (_syncObject)
                _isDialogOpen = false;
            ShowDialogAsync();
        };
    }
}