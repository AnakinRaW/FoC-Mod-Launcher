using System.ComponentModel;
using System.Threading.Tasks;
using AnakinRaW.AppUpdaterFramework.Metadata.Update;
using AnakinRaW.AppUpdaterFramework.ViewModels.ProductStates;

namespace AnakinRaW.AppUpdaterFramework.ViewModels.Designer;

[EditorBrowsable(EditorBrowsableState.Never)]
public class DesignerUpdateAvailableStateViewModel : IUpdateAvailableStateViewModel
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public IUpdateCatalog UpdateCatalog { get; } = null!;
    public string? CurrentVersion { get; } = "1.2.3.4";
    public string? AvailableVersion { get; } = "4.3.2.1";

    public Task InitializeAsync()
    {
        throw new System.NotImplementedException();
    }
}