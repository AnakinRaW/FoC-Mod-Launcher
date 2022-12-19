using CommunityToolkit.Mvvm.ComponentModel;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

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