using System.Threading.Tasks;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FocLauncher.ViewModels;

internal partial class UpdateWindowViewModel : ModalWindowViewModel, IUpdateWindowViewModel
{
    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string? _loadingText;

    public UpdateWindowViewModel()
    {
        Title = "Launcher Update";
        HasMaximizeButton = false;
        HasMinimizeButton = false;
        IsResizable = false;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public void OnClosing(CancelEventArgs e)
    {
    }
}