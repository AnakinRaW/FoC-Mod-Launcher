using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.StatusBar;

public interface IThreadedStatusBarFactory
{
    FrameworkElement CreateStatusBar();
}