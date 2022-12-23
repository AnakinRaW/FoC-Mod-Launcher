using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

internal interface INonClientArea
{
    int HitTest(Point point);
}