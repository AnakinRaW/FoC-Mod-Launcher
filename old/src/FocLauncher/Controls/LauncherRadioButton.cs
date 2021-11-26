using System.Windows;
using System.Windows.Controls;

namespace FocLauncher.Controls
{
    internal class LauncherRadioButton : RadioButton
    {
        static LauncherRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LauncherRadioButton), new FrameworkPropertyMetadata(typeof(LauncherRadioButton)));
        }
    }
}