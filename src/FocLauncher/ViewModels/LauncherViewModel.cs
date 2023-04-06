using System;
using System.Threading.Tasks;
using AnakinRaW.ApplicationBase.ViewModels.Dialogs;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Dialog;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.StatusBar;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FocLauncher.ViewModels;

public class LauncherViewModel : ApplicationViewModelBase, IApplicationViewModel
{
    public LauncherViewModel(IServiceProvider serviceProvider, IStatusBarViewModel statusBar) : base(statusBar, serviceProvider)
    {
    }

    public override Task InitializeAsync()
    {
        return Task.Run(() =>
        {
            ServiceProvider.GetRequiredService<IViewModelPresenter>().ViewRequested +=
                async (_, newViewModel) =>
                {
                    try
                    {
                        Logger?.LogTrace(
                            $"Navigation requested from {CurrentViewModel?.GetType().Name ?? nameof(LauncherViewModel)} to {newViewModel.GetType().Name}");
                        CurrentViewModel = newViewModel;
                        await CurrentViewModel.InitializeAsync();
                    }
                    catch (Exception ex)
                    {
                        Logger?.LogError(ex, ex.Message);
                        ShowErrorDialog(ex.Message);
                    }
                };
        });
    }

    private void ShowErrorDialog(string message)
    {
        Logger?.LogTrace("An exception happened while initializing LauncherViewModel. Showing the error window.");
        var dialogService = ServiceProvider.GetRequiredService<IQueuedDialogService>();
        const string header = "Initializing the launcher resulted in an error.";
        dialogService.ShowDialog(new ErrorMessageDialogViewModel(header, message, ServiceProvider));
    }
}