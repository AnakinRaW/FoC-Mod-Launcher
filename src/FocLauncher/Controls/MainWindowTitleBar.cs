using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace FocLauncher.Controls
{
    public sealed class MainWindowTitleBar : Border, INonClientArea
    {
        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }

        int INonClientArea.HitTest(Point point)
        {
            return 2;
        }
        
        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            if (e.Handled)
                return;
            if (PresentationSource.FromVisual(this) is HwndSource source)
                ShadowChromeWindow.ShowWindowMenu(source, this, Mouse.GetPosition(this), RenderSize);
            e.Handled = true;
        }
    }
}
