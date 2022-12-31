using System.Windows;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.StatusBar;

namespace FocLauncher.Controls;

internal class LauncherStatusBarFactory : IStatusBarFactory
{
    public FrameworkElement CreateStatusBar(IStatusBarViewModel viewModel)
    {
        return new LauncherStatusBar();
    }
}