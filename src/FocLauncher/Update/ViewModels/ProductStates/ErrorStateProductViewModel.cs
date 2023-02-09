using System;
using AnakinRaW.AppUpaterFramework.Metadata.Product;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

namespace FocLauncher.Update.ViewModels.ProductStates;

public class ErrorStateProductViewModel : ViewModelBase, IInstalledProductStateViewModel
{
    public string? Version { get; }

    public string ErrorMessage { get; }

    public ErrorStateProductViewModel(IInstalledProduct installedProduct, string error, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Version = installedProduct.Version?.ToString();
        ErrorMessage = error;
    }
}