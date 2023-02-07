﻿using System;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using AnakinRaW.ProductUpdater;
using AnakinRaW.ProductUpdater.Catalog;
using AnakinRaW.ProductUpdater.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using FocLauncher.Utilities;
using Microsoft.Extensions.DependencyInjection;

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
            return Status switch
            {
                UpdateStatus.CheckingForUpdate => "Checking for updates...",
                UpdateStatus.UpdateAvailable => "Updates are available.",
                UpdateStatus.NoUpdate => "No update available.",
                UpdateStatus.Updating => "Updating Product...",
                UpdateStatus.Failed => "Failed to update or get update information.",
                UpdateStatus.Cancelled => "Operation cancelled.",
                _ => "No update available."
            };
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

    private Task Refresh(IUpdateCatalog? e)
    {
        return Task.Run(() =>
        {
            if (e is null || !e.RequiresUpdate())
                Status = UpdateStatus.NoUpdate;
            else
                Status = UpdateStatus.UpdateAvailable;
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