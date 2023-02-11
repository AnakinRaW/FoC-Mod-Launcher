using System.ComponentModel;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ProgressBar;
using FocLauncher.Update.ViewModels.ProductStates;

namespace FocLauncher.Update.ViewModels.Designer;

[EditorBrowsable(EditorBrowsableState.Never)]
public class DesignerUpdatingStateViewModel : IUpdatingStateViewModel
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public Task InitializeAsync()
    {
        throw new System.NotImplementedException();
    }

    public IProgressBarViewModel DownloadProgressBarViewModel { get; }
    public IProgressBarViewModel UpdateProgressBarViewModel { get; }
}