using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using Sklavenwalker.CommonUtilities.Wpf.NativeMethods;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public class CustomResizeGrip : ResizeGrip
{
    private const int SizeBottomLeft = 7;
    private const int SizeBottomRight = 8;

    static CustomResizeGrip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomResizeGrip), new FrameworkPropertyMetadata(typeof(CustomResizeGrip)));
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.ChangedButton != MouseButton.Left)
            return;
        var num = FlowDirection == FlowDirection.LeftToRight ? SizeBottomRight : SizeBottomLeft;
        if (PresentationSource.FromVisual(this) is not HwndSource hwndSource)
            return;
        User32.SendMessage(hwndSource.Handle, 274, (IntPtr)(61440 + num), IntPtr.Zero);
    }
}