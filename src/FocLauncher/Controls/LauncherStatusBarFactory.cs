using System.Windows;
using Sklavenwalker.CommonUtilities.Wpf.ApplicationFramework.Services;

namespace FocLauncher.Controls;

internal class LauncherStatusBarFactory : IStatusBarFactory
{
    public UIElement CreateStatusBar()
    {
        return new LauncherStatusBar();
    }
}