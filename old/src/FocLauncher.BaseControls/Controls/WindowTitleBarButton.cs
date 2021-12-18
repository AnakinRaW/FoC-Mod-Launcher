using System.Windows;

namespace FocLauncher.Controls
{
    public class WindowTitleBarButton : GlyphButton
    { 
        static WindowTitleBarButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowTitleBarButton), new FrameworkPropertyMetadata(typeof(WindowTitleBarButton)));
        }

        protected override int HitTestCore(Point point) => this.HitTestResult;

    }
}
