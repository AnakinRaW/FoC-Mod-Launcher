using System.Windows;
using System.Windows.Controls;

namespace AnakinRaW.CommonUtilities.Wpf.Controls;

public class ThemedSeparator : Separator
{
    static ThemedSeparator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ThemedSeparator), new FrameworkPropertyMetadata(typeof(ThemedSeparator)));
    }
}