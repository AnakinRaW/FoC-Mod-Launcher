using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Sklavenwalker.CommonUtilities.Wpf.Utilities;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

internal class MainWindowTitleBar : Border, INonClientArea
{
    public int HitTest(Point point)
    {
        var ancestor = this.FindAncestor<Window>();
        return ancestor is { WindowState: WindowState.Normal } && PointFromScreen(point).Y < ShadowChromeWindow.LogicalResizeBorder.Height ? 12 : 2;
    }

    protected override void OnContextMenuOpening(ContextMenuEventArgs e)
    {
        if (e.Handled)
            return;
        if (PresentationSource.FromVisual(this) is HwndSource source) 
            ShadowChromeWindow.ShowWindowMenu(source, this, Mouse.GetPosition(this),RenderSize);
        e.Handled = true;
    }

    protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
    {
        return new PointHitTestResult(this, hitTestParameters.HitPoint);
    }
}