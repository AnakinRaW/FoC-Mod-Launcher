using System.Windows;
using System.Windows.Controls;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public class ThemedExpander : Expander
{
    static ThemedExpander()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ThemedExpander),
            new FrameworkPropertyMetadata(typeof(ThemedExpander)));
    }
}