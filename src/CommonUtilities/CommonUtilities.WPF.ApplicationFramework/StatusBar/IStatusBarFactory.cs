using System.Windows;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.StatusBar;

public interface IStatusBarFactory
{
    FrameworkElement CreateStatusBar(IStatusBarViewModel viewModel);
}