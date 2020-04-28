using System.Windows;
using System.Windows.Controls;

namespace FocLauncher.Controls
{
    internal class StatusBarButton : Button
    {
        static StatusBarButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StatusBarButton), new FrameworkPropertyMetadata(typeof(StatusBarButton)));
        }
    }
}
