using System;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

namespace FocLauncher.Update.ViewModels.ProductStates;

public class InstalledStateViewModel : ViewModelBase, IInstalledStateViewModel
{
    public string? Version { get; }

    public InstalledStateViewModel(IInstalledProduct installedProduct, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Version = installedProduct.Version?.ToString();
    }
}