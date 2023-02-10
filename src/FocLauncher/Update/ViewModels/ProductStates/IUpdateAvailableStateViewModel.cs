using AnakinRaW.AppUpaterFramework.Metadata.Update;

namespace FocLauncher.Update.ViewModels.ProductStates;

public interface IUpdateAvailableStateViewModel : IProductStateViewModel
{
    IUpdateCatalog UpdateCatalog { get; }

    string? CurrentVersion { get; }

    string? AvailableVersion { get; }
}