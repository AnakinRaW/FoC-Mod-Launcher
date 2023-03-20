using AnakinRaW.AppUpdaterFramework.ViewModels.Progress;

namespace AnakinRaW.AppUpdaterFramework.ViewModels.ProductStates;

internal interface IUpdatingStateViewModel : IProductStateViewModel
{
    IProgressViewModel ProgressViewModel { get; }
}