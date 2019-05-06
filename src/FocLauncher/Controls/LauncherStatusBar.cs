using System.Windows;
using System.Windows.Controls;

namespace FocLauncher.Core.Controls
{
    public class LauncherStatusBar : Grid
    {
        static LauncherStatusBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LauncherStatusBar), new FrameworkPropertyMetadata(typeof(LauncherStatusBar)));
        }
    }
}
