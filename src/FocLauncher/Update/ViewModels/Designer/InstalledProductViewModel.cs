using System.ComponentModel;
using System.Threading.Tasks;
using FocLauncher.Update.ViewModels;
using Sklavenwalker.CommonUtilities.Wpf.Imaging;

namespace FocLauncher.Update.ViewModels.Designer;

public class InstalledProductViewModel : IInstalledProductViewModel
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string DisplayName => "Test Product";
    public ImageKey Icon { get; }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}