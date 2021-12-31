namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public interface IMainWindowViewModel : IWindowViewModel
{
    TaskBarIconProgressState ProgressState { get; set; }
    
    IStatusBarViewModel StatusBar { get; }
}