namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public class MainWindowViewModel : WindowViewModel
{
    public MainWindowViewModel(StatusBarViewModel statusBar)
    {
        StatusBar = statusBar;
    }

    public MainWindowViewModel() : this(new InvisibleStatusBarViewModel())
    {
    }

    public StatusBarViewModel StatusBar { get; }

    private class InvisibleStatusBarViewModel : StatusBarViewModel
    {
    }
}