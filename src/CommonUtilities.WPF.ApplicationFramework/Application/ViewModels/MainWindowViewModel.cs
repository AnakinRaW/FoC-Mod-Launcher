using CommunityToolkit.Mvvm.ComponentModel;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.StatusBar;
using WindowViewModel = Sklavenwalker.CommonUtilities.Wpf.Controls.WindowViewModel;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

public partial class MainWindowViewModel : WindowViewModel, IMainWindowViewModel
{
    [ObservableProperty]
    private TaskBarIconProgressState _progressState;

    public IStatusBarViewModel StatusBar { get; }

    public MainWindowViewModel(IStatusBarViewModel statusBar)
    {
        StatusBar = statusBar;
    }

    public MainWindowViewModel() : this(new InvisibleStatusBarViewModel())
    {
    }
}