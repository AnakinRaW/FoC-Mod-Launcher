using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public interface INonClientArea
{
    int HitTest(Point point);
}