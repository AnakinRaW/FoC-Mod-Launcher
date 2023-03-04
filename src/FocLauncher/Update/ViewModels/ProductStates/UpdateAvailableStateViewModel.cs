using System;
using AnakinRaW.AppUpdaterFramework.Metadata.Product;
using AnakinRaW.AppUpdaterFramework.Metadata.Update;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using Validation;

namespace FocLauncher.Update.ViewModels.ProductStates;

public class UpdateAvailableStateViewModel : ViewModelBase, IUpdateAvailableStateViewModel
{
    public IUpdateCatalog UpdateCatalog { get; }

    public string? CurrentVersion { get; }

    public string? AvailableVersion { get; }

    public UpdateAvailableStateViewModel(IInstalledProduct installedProduct, IUpdateCatalog updateCatalog, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Requires.NotNull(updateCatalog, nameof(updateCatalog));
        UpdateCatalog = updateCatalog;
        CurrentVersion = installedProduct.Version?.ToString();
        AvailableVersion = updateCatalog.UpdateReference.Version?.ToString();
    }
}