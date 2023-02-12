namespace FocLauncher.Update.ViewModels.ProductStates;

internal interface IUpdatingStateViewModel : IProductStateViewModel
{
    IProgressViewModel ProgressViewModel { get; }
}