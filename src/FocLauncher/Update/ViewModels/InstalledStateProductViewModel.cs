using System;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using AnakinRaW.ProductMetadata;

namespace FocLauncher.Update.ViewModels;

public class InstalledStateProductViewModel : ViewModelBase, IInstalledProductStateViewModel
{
    public string? Version { get; }

    public InstalledStateProductViewModel(IInstalledProduct installedProduct, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Version = installedProduct.Version?.ToString();
    }
}