using System.Windows;

namespace FocLauncher.Controls
{
    public class WindowTitleBarButton : GlyphButton, INonClientArea
    { 
        static WindowTitleBarButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowTitleBarButton), new FrameworkPropertyMetadata(typeof(WindowTitleBarButton)));
        }

        int INonClientArea.HitTest(Point point)
        {
            return 1;
        }
    }
}
