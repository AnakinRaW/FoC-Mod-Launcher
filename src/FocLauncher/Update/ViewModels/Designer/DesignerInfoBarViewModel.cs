using System.ComponentModel;
using System.Threading.Tasks;

namespace FocLauncher.Update.ViewModels.Designer;

[EditorBrowsable(EditorBrowsableState.Never)]
public class DesignerInfoBarViewModel : IUpdateInfoBarViewModel
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public UpdateStatus Status { get; set; }
    public string Text => "Status";
    public bool IsCheckingForUpdates => true;

    public void Dispose()
    {
    }
}