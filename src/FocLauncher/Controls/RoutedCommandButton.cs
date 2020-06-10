using System;
using System.Windows;
using System.Windows.Controls;

namespace FocLauncher.Controls
{
    internal class RoutedCommandButton : Button
    {
        static RoutedCommandButton()
        {
            CommandProperty.OverrideMetadata(typeof(RoutedCommandButton),
                new FrameworkPropertyMetadata((d, e) =>
                    RefreshCanExecute(e.Property, typeof(RoutedCommandButton), d, e.NewValue)));
        }

        public RoutedCommandButton()
        {
            PresentationSource.AddSourceChangedHandler(this, OnSourceChanged);
        }

        private void OnSourceChanged(object sender, SourceChangedEventArgs e)
        {
            if (e.NewSource == null)
                return;
            RefreshCanExecute(CommandProperty, typeof(RoutedCommandButton), this, Command);
        }

        private static void RefreshCanExecute(DependencyProperty commandProperty, Type controlType, DependencyObject d, object value)
        {
            if (value == null)
                return;
            commandProperty.GetMetadata(controlType).PropertyChangedCallback(d, new DependencyPropertyChangedEventArgs(commandProperty, value, null));
        }
    }
}