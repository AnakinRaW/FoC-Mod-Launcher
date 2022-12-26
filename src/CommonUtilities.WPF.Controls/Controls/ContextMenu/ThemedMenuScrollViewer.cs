using System.Windows;
using System.Windows.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public class ThemedMenuScrollViewer : ScrollViewer
{
    static ThemedMenuScrollViewer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ThemedMenuScrollViewer), new FrameworkPropertyMetadata(typeof(ThemedMenuScrollViewer)));
    }
}