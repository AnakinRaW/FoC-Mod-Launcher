using System.Windows;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.StatusBar;

namespace FocLauncher.Controls;

internal class LauncherStatusBarFactory : IStatusBarFactory
{
    public UIElement CreateStatusBar()
    {
        return new LauncherStatusBar();
    }
}