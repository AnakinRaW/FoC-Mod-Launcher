using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using FocLauncher.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using Validation;

namespace FocLauncher.ViewModels;

public partial class ApplicationViewModel : MainWindowViewModel, IApplicationViewModel
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger? _logger;

    [ObservableProperty]
    private ILauncherViewModel _currentViewModel;

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
                        throw new Exception();
                        _logger?.LogTrace(
                            $"Navigation requested from {CurrentViewModel?.GetType().Name ?? nameof(ApplicationViewModel)} to {newViewModel.GetType().Name}");
                        CurrentViewModel = newViewModel;
                        await CurrentViewModel.InitializeAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, ex.Message);
                        ShowErrorDialog();
                    }
                };
        });
    }

    private void ShowErrorDialog()
    {
        var dialog = new ThemedDialog();

        var a = new ButtonViewModel("test", new CommandDefinition("123")
        {
            Command = new RelayCommand(() => MessageBox.Show("a"), () => false)
        });
        var b = new ButtonViewModel("test", new CommandDefinition("456")
        {
            Command = new RelayCommand(() => dialog.Close())
        })
        {
            IsDefault = true,
            IsCancel = true
        };
        var c = new DropDownButtonViewModel("test", new CommandDefinition("789"));

        var buttons = new List<IButtonViewModel> { a, b, c };
        
        dialog.Initialize(new ErrorDialogViewModel(buttons));
        dialog.ShowModal();
        _logger?.LogTrace("An exception happened while Initializing VM. Showing the error dialog.");
    }
}