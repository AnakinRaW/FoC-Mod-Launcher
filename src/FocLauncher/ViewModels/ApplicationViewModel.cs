using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using FocLauncher.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Validation;

namespace FocLauncher.ViewModels;

public partial class ApplicationViewModel : MainWindowViewModel, IApplicationViewModel
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger? _logger;

    [ObservableProperty]
    private ILauncherViewModel? _currentViewModel;

    public ApplicationViewModel(IServiceProvider serviceProvider, IStatusBarViewModel statusBar) : base(statusBar)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
    }

    public Task InitializeAsync()
    {
        return Task.Run(() =>
        {
            _serviceProvider.GetRequiredService<IViewModelPresenter>().ViewRequested +=
                async (_, newViewModel) =>
                {
                    try
                    {
                        _logger?.LogTrace(
                            $"Navigation requested from {CurrentViewModel?.GetType().Name ?? nameof(ApplicationViewModel)} to {newViewModel.GetType().Name}");
                        CurrentViewModel = newViewModel;
                        await CurrentViewModel.InitializeAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, ex.Message);
                        ShowErrorDialog(ex.Message);
                    }
                };
        });
    }

    private void ShowErrorDialog(string message)
    {
        _logger?.LogTrace("An exception happened while initializing ApplicationViewModel. Showing the error window.");
        var dialogService = _serviceProvider.GetRequiredService<IQueuedDialogService>();
        const string header = "Initializing the launcher resulted in an error.";
        dialogService.ShowDialog(new ErrorMessageDialogViewModel(header, message, _serviceProvider));
    }
}