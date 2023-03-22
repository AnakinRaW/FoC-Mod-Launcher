using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using AnakinRaW.AppUpdaterFramework.Metadata.Update;
using AnakinRaW.AppUpdaterFramework.Updater;
using AnakinRaW.AppUpdaterFramework.Utilities;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace AnakinRaW.AppUpdaterFramework.ViewModels;

public partial class UpdateInfoBarViewModel : ViewModelBase, IUpdateInfoBarViewModel
{
    private readonly IAppDispatcher _dispatcher;

    [ObservableProperty] private UpdateStatus _status;

    [NotifyChangedIsLinkedToProperty(nameof(Status))]
    public string Text
    {
        get
        {
            return Status switch
            {
                UpdateStatus.CheckingForUpdate => "Checking for updates...",
                UpdateStatus.UpdateAvailable => "Updates are available.",
                UpdateStatus.NoUpdate => "No update available.",
                UpdateStatus.Updating => "Updating Product...",
                UpdateStatus.FailedChecking => "FailedChecking to get update information.",
                _ => "No update available."
            };
        }
    }

    [NotifyChangedIsLinkedToProperty(nameof(Status))]
    public bool IsCheckingForUpdates => Status == UpdateStatus.CheckingForUpdate;

    public UpdateInfoBarViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _dispatcher = serviceProvider.GetRequiredService<IAppDispatcher>();
        var updateService = serviceProvider.GetRequiredService<IUpdateService>();
        updateService.CheckingForUpdatesStarted += OnCheckingUpdatesStarted;
        updateService.CheckingForUpdatesCompleted += OnCheckingUpdatesCompleted;
        updateService.UpdateStarted += OnUpdateStarted;
        updateService.UpdateCompleted += OnUpdateCompleted;
    }

    private void OnUpdateCompleted(object sender, EventArgs e)
    {
        Refresh(null).Forget();
    }

    private void OnUpdateStarted(object sender, IUpdateSession e)
    {
        Status = UpdateStatus.Updating;
        _dispatcher.BeginInvoke(DispatcherPriority.Background, CommandManager.InvalidateRequerySuggested);
    }

    private Task Refresh(IUpdateCatalog? e)
    {
        return Task.Run(() =>
        {
            if (e is null || e.Action == UpdateCatalogAction.None)
                Status = UpdateStatus.NoUpdate;
            else if (e.Action == UpdateCatalogAction.Update)
                Status = UpdateStatus.UpdateAvailable;
            else
                Status = UpdateStatus.FailedChecking;
        });
    }

    private void OnCheckingUpdatesCompleted(object sender, IUpdateCatalog? e)
    {
        Refresh(e).Forget();
    }

    private void OnCheckingUpdatesStarted(object sender, EventArgs e)
    {
        Status = UpdateStatus.CheckingForUpdate;
    }
}