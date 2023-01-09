using System.Windows;
using AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.StatusBar;
using FocLauncher.Controls;

namespace FocLauncher.Services;

internal class LauncherStatusBarFactory : IStatusBarFactory
{
    public FrameworkElement CreateStatusBar(IStatusBarViewModel viewModel)
    {
        return new LauncherStatusBar();
    }
}