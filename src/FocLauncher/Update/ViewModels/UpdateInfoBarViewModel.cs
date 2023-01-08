using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using Sklavenwalker.ProductUpdater.Catalog;
using Sklavenwalker.ProductUpdater.Services;

namespace FocLauncher.Update.ViewModels;

public partial class UpdateInfoBarViewModel : ViewModelBase, IUpdateInfoBarViewModel
{
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty] private UpdateStatus _status;

    [NotifyChangedIsLinkedToProperty(nameof(Status))]
    public string Text
    {
        get
        {
            switch (Status)
            {
                case UpdateStatus.CheckingForUpdate:
                    return "Checking for updates...";
                case UpdateStatus.UpdateAvailable:
                    return "Updates are available.";
                case UpdateStatus.NoUpdate:
                    return "No update available.";
                case UpdateStatus.Updating:
                    return "Updating Product...";
                default:
                    return "No update available.";
            }
        }
    }

    [NotifyChangedIsLinkedToProperty(nameof(Status))]
    public bool IsCheckingForUpdates => Status == UpdateStatus.CheckingForUpdate;

    public UpdateInfoBarViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _serviceProvider = serviceProvider;
        var updateService = serviceProvider.GetRequiredService<IProductUpdateProviderService>();
        updateService.CheckingForUpdatesStarted += OnCheckingUpdatesStarted;
        updateService.CheckingForUpdatesCompleted += OnCheckingUpdatesCompleted;
    }

    private Task Refresh()
    {
        return Task.Run(() =>
        {
            Status = UpdateStatus.NoUpdate;
        });
    }

    private void OnCheckingUpdatesCompleted(object sender, IUpdateCatalog? e)
    {
        Refresh();
    }

    private void OnCheckingUpdatesStarted(object sender, EventArgs e)
    {
        Status = UpdateStatus.CheckingForUpdate;
    }
}