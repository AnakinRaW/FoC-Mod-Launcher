using System.Windows;
using System.Windows.Controls;

namespace FocLauncher.Controls
{
    internal class LauncherCheckBox : CheckBox
    {
        static LauncherCheckBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LauncherCheckBox), new FrameworkPropertyMetadata(typeof(LauncherCheckBox)));
        }
    }
}
