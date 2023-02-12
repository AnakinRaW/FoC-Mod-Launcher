using System.ComponentModel;
using System.Threading.Tasks;
using FocLauncher.Update.ViewModels.ProductStates;

namespace FocLauncher.Update.ViewModels.Designer;

[EditorBrowsable(EditorBrowsableState.Never)]
internal class DesignerUpdatingStateViewModel : IUpdatingStateViewModel
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public Task InitializeAsync()
    {
        throw new System.NotImplementedException();
    }

    public IProgressViewModel ProgressViewModel { get; }
}