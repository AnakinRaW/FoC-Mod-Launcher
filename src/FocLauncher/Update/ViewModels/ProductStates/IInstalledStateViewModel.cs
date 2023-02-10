namespace FocLauncher.Update.ViewModels.ProductStates;

public interface IInstalledStateViewModel : IProductStateViewModel
{
    string? Version { get; }
}