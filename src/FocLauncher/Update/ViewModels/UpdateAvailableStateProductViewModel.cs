using System;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using AnakinRaW.ProductMetadata;
using AnakinRaW.ProductUpdater.Catalog;
using Validation;

namespace FocLauncher.Update.ViewModels;

public class UpdateAvailableStateProductViewModel : ViewModelBase, IInstalledProductStateViewModel
{
    public IUpdateCatalog UpdateCatalog { get; }

    public string? CurrentVersion { get; }

    public string? AvailableVersion { get; }

    public UpdateAvailableStateProductViewModel(IInstalledProduct installedProduct, IUpdateCatalog updateCatalog, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Requires.NotNull(updateCatalog, nameof(updateCatalog));
        UpdateCatalog = updateCatalog;
        CurrentVersion = installedProduct.Version?.ToString();
        AvailableVersion = updateCatalog.Product.Version?.ToString();
    }
}