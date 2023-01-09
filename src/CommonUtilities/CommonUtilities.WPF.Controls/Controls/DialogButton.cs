using System.Windows;
using System.Windows.Controls;

namespace AnakinRaW.CommonUtilities.Wpf.Controls;

public class DialogButton : Button
{
    static DialogButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogButton), new FrameworkPropertyMetadata(typeof(DialogButton)));
    }
}