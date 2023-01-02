using System.Windows;
using FocLauncher.Controls;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.StatusBar;

namespace FocLauncher.Services;

internal class LauncherStatusBarFactory : IStatusBarFactory
{
    public FrameworkElement CreateStatusBar(IStatusBarViewModel viewModel)
    {
        return new LauncherStatusBar();
    }
}