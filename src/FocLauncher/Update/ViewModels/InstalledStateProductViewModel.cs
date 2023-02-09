using System;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

namespace FocLauncher.Update.ViewModels;

public class InstalledStateProductViewModel : ViewModelBase, IInstalledProductStateViewModel
{
    public string? Version { get; }

    public InstalledStateProductViewModel(IInstalledProduct installedProduct, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Version = installedProduct.Version?.ToString();
    }
}