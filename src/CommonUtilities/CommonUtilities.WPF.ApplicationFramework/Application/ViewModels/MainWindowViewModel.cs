using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.StatusBar;
using CommunityToolkit.Mvvm.ComponentModel;
using WindowViewModel = AnakinRaW.CommonUtilities.Wpf.Controls.WindowViewModel;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

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