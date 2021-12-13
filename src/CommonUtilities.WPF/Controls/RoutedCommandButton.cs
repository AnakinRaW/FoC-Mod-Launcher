using System.Windows;
using System.Windows.Controls;
using Sklavenwalker.CommonUtilities.Wpf.Utils;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public class RoutedCommandButton : Button
{
    static RoutedCommandButton()
    {
        RoutedCommandUnsubscriber.OverrideMetadata(CommandProperty, typeof(RoutedCommandButton));
    }

    public RoutedCommandButton()
    {
        PresentationSource.AddSourceChangedHandler(this, OnSourceChanged);
    }

    private void OnSourceChanged(object sender, SourceChangedEventArgs e)
    {
        if (e.NewSource == null)
            return;
        RoutedCommandUnsubscriber.RefreshCanExecute(CommandProperty, typeof(RoutedCommandButton), this, Command);
    }
}