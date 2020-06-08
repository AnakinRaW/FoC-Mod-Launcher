using System.Windows;

namespace FocLauncher.Controls
{
    internal interface INonClientArea
    {
        int HitTest(Point point);
    }
}