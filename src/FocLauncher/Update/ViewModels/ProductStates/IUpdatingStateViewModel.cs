using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ProgressBar;

namespace FocLauncher.Update.ViewModels.ProductStates;

public interface IUpdatingStateViewModel : IProductStateViewModel
{
    IProgressBarViewModel DownloadProgressBarViewModel { get; }
    IProgressBarViewModel UpdateProgressBarViewModel { get; }
}