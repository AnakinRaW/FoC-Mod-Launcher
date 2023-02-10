using System.ComponentModel;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.CommonUtilities.Wpf.Imaging;
using FocLauncher.Update.ViewModels.ProductStates;

namespace FocLauncher.Update.ViewModels.Designer;

[EditorBrowsable(EditorBrowsableState.Never)]
public class DesignerProductViewModel : IProductViewModel
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string DisplayName => "Test Product";
    public ImageKey Icon { get; }
    public ICommandDefinition? Action { get; }
    public IProductStateViewModel StateViewModel { get; } = new DesignerUpdateAvailableStateViewModel();

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}