using System.ComponentModel;
using System.Threading.Tasks;
using AnakinRaW.AppUpdaterFramework.ViewModels.ProductStates;

namespace AnakinRaW.AppUpdaterFramework.ViewModels.Designer;

[EditorBrowsable(EditorBrowsableState.Never)]
public class DesignerInstalledStateViewModel : IInstalledStateViewModel
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string? Version { get; } = "1.2.3.4";

    public Task InitializeAsync()
    {
        throw new System.NotImplementedException();
    }
}