using System;
using System.Windows;

namespace AnakinRaW.CommonUtilities.Wpf.Utilities;

internal static class RoutedCommandUnsubscriber
{
    public static void OverrideMetadata(DependencyProperty commandProperty, Type controlType)
    {
        commandProperty.OverrideMetadata(controlType,
            new FrameworkPropertyMetadata((d, e) => RefreshCanExecute(e.Property, controlType, d, e.NewValue)));
    }

    internal static void RefreshCanExecute(DependencyProperty commandProperty, Type controlType, DependencyObject d, object? value)
    {
        if (value == null)
            return;
        commandProperty.GetMetadata(controlType).PropertyChangedCallback(d, new DependencyPropertyChangedEventArgs(commandProperty, value, null));
    }
}