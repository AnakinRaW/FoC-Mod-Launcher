using System.Windows;

namespace AnakinRaW.CommonUtilities.Wpf.Controls;

public interface INonClientArea
{
    int HitTest(Point point);
}