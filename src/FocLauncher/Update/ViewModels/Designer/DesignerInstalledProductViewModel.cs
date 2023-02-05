using System.ComponentModel;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Input;
using AnakinRaW.CommonUtilities.Wpf.Imaging;

namespace FocLauncher.Update.ViewModels.Designer;

[EditorBrowsable(EditorBrowsableState.Never)]
public class DesignerInstalledProductViewModel : IInstalledProductViewModel
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string DisplayName => "Test Product";
    public ImageKey Icon { get; }
    public ICommandDefinition? Action { get; }
    public IInstalledProductStateViewModel StateViewModel { get; }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}