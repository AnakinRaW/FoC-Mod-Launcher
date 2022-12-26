using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.StatusBar;
using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Application;

public interface IMainWindowViewModel : IWindowViewModel
{
    TaskBarIconProgressState ProgressState { get; set; }

    IStatusBarViewModel StatusBar { get; }
}