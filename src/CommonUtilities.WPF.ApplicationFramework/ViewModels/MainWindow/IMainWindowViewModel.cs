using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

public interface IMainWindowViewModel : IWindowViewModel
{
    TaskBarIconProgressState ProgressState { get; set; }
    
    IStatusBarViewModel StatusBar { get; }
}