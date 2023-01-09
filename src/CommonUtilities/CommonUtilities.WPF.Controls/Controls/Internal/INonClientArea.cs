using System.Windows;

namespace AnakinRaW.CommonUtilities.Wpf.Controls;

internal interface INonClientArea
{
    int HitTest(Point point);
}