using System.Windows;
using System.Windows.Controls.Primitives;
using Sklavenwalker.CommonUtilities.Wpf.Utilities;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

public class RoutedCommandRepeatButton : RepeatButton
{
    static RoutedCommandRepeatButton()
    {
        RoutedCommandUnsubscriber.OverrideMetadata(CommandProperty, typeof(RoutedCommandRepeatButton));
    }

    public RoutedCommandRepeatButton()
    {
        PresentationSource.AddSourceChangedHandler(this, OnSourceChanged);
    }

    private void OnSourceChanged(object sender, SourceChangedEventArgs e)
    {
        if (e.NewSource == null)
            return;
        RoutedCommandUnsubscriber.RefreshCanExecute(CommandProperty, typeof(RoutedCommandRepeatButton), this, Command);
    }
}