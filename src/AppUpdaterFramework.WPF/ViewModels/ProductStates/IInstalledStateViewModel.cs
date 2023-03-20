namespace AnakinRaW.AppUpdaterFramework.ViewModels.ProductStates;

public interface IInstalledStateViewModel : IProductStateViewModel
{
    string? Version { get; }
}