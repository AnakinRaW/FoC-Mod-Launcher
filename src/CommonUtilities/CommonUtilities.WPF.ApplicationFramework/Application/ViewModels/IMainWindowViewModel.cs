using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.StatusBar;
using AnakinRaW.CommonUtilities.Wpf.Controls;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.ViewModels;

public interface IMainWindowViewModel : IWindowViewModel
{
    TaskBarIconProgressState ProgressState { get; set; }

    IStatusBarViewModel StatusBar { get; }
}