using System.Windows;
using System.Windows.Controls;

namespace FocLauncher.Controls
{
    internal class LauncherExpander : Expander
    {
        static LauncherExpander()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LauncherExpander), new FrameworkPropertyMetadata(typeof(LauncherExpander)));
        }
    }
}