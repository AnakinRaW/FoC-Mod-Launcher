using System.ComponentModel;
using System.Threading.Tasks;

namespace FocLauncher.ViewModels.Designer;

internal class MainPageViewModel : IMainPageViewModel
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsLoading { get; }
    public string? LoadingText { get; }

    public Task InitializeAsync()
    {
        throw new System.NotImplementedException();
    }
}