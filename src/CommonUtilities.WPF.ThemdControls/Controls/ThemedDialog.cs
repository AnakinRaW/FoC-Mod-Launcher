using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public class ThemedDialog : DialogWindowBase
{
    static ThemedDialog()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ThemedDialog), new FrameworkPropertyMetadata(typeof(ThemedDialog)));
    }
}