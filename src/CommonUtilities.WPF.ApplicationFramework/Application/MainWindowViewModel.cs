using CommunityToolkit.Mvvm.ComponentModel;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.StatusBar;
using Sklavenwalker.CommonUtilities.Wpf.Controls.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Application;

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