using System.Windows;
using System.Windows.Controls.Primitives;

namespace FocLauncher.Core.Controls
{
    public class LauncherStatusBar : StatusBar
    {
        static LauncherStatusBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LauncherStatusBar), new FrameworkPropertyMetadata(typeof(LauncherStatusBar)));
        }
    }
}
