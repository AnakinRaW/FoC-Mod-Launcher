using System.Windows;
using Sklavenwalker.CommonUtilities.Wpf.Controls;

namespace FocLauncher.Controls;

internal class LauncherStatusBarFactory : IStatusBarFactory
{
    public UIElement CreateStatusBar()
    {
        return new LauncherStatusBar();
    }
}