using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Markup;
using AnakinRaW.CommonUtilities.Wpf.DPI;
using AnakinRaW.CommonUtilities.Wpf.NativeMethods;
using AnakinRaW.CommonUtilities.Wpf.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Validation;

namespace AnakinRaW.CommonUtilities.Wpf.ApplicationFramework.Utilities;

internal class MenuPopupPositionerExtension : MarkupExtension
{
    private const double BorderThickness = 1.0;

    public string ElementName { get; set; } = null!;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var service = serviceProvider.GetRequiredService<IProvideValueTarget>();
        Assumes.Present(service);
        if (service.TargetObject is not FrameworkElement border)
            return this;
        var popup = border.FindAncestor<Popup>();
        if (popup != null)
            popup.Opened += (_, _) =>
            {
                var name = border.FindName(ElementName) as FrameworkElement;
                var screen = name.PointToScreen(new Point(0.0, 0.0));
                User32.GetWindowRect(((HwndSource)PresentationSource.FromVisual(popup.Child)).Handle, out var lpRect);
                if (popup.Placement is PlacementMode.Left or PlacementMode.Right)
                {
                    border.Visibility = Visibility.Collapsed;
                    if (popup.Placement == PlacementMode.Left && lpRect.Left > screen.X)
                    {
                        popup.HorizontalOffset = 2.0;
                    }
                    else
                    {
                        if (popup.Placement != PlacementMode.Right || lpRect.Left >= screen.X)
                            return;
                        popup.HorizontalOffset = -2.0;
                    }
                }
                else if (screen.Y > lpRect.Top)
                {
                    border.Visibility = Visibility.Hidden;
                }
                else
                {
                    var left = BorderThickness + popup.DeviceToLogicalUnitsX(screen.X - lpRect.Left);
                    var val2 = Math.Max(0.0, popup.DeviceToLogicalUnitsX(lpRect.Left + lpRect.Width - screen.X));
                    border.Margin = new Thickness(left, 0.0, 0.0, 0.0);
                    border.Width = Math.Min(name.ActualWidth - 2.0, val2);
                    border.Visibility = Visibility.Visible;
                }
            };
        return new Thickness(1.0);
    }
}