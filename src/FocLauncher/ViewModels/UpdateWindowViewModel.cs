using System;
using System.Threading.Tasks;
using Sklavenwalker.CommonUtilities.Wpf.Controls;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;
using Sklavenwalker.CommonUtilities.Wpf.Input;

namespace FocLauncher.ViewModels;

internal partial class UpdateWindowViewModel : ModalWindowViewModel, ILoadingViewModel
{
    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string? _loadingText;

    public ICommand ClickCommand => new DelegateCommand(() => throw new Exception());

    public UpdateWindowViewModel()
    {
        HasMaximizeButton = false;
        HasMinimizeButton = false;
    }

    public Task InitializeAsync()
    {
        return Task.Run(async () =>
        {
            await Task.Delay(3000);
            throw new Exception("Test");
        });
    }

    public void OnClosing(CancelEventArgs e)
    {
    }
}