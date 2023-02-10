namespace FocLauncher.Update.ViewModels.ProductStates;

public interface IErrorStateViewModel : IProductStateViewModel
{
    string? Version { get; }

    string ErrorMessage { get; }
}