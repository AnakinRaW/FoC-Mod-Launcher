using System.Windows;
using System.Windows.Controls;

namespace AnakinRaW.CommonUtilities.Wpf.Controls;

public class ThemedButton : Button, INonClientArea
{
    static ThemedButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ThemedButton), new FrameworkPropertyMetadata(typeof(ThemedButton)));
    }

    public int HitTest(Point point)
    {
        return 1;
    }
}