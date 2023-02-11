using System.Windows;
using System.Windows.Controls;
using AnakinRaW.CommonUtilities.Wpf.Imaging;

namespace AnakinRaW.CommonUtilities.Wpf.Controls;

public class ThemedProgressBar : Control
{
    public static readonly DependencyProperty FinishedIconProperty = DependencyProperty.Register(
        nameof(FinishedIcon), typeof(ImageKey), typeof(ThemedProgressBar), new PropertyMetadata(default(ImageKey)));

    public static readonly DependencyProperty FooterMessageProperty = DependencyProperty.Register(
        nameof(FooterMessage), typeof(string), typeof(ThemedProgressBar), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(double), typeof(ThemedProgressBar), new PropertyMetadata(default(double)));

    public static readonly DependencyProperty ProgressBarHeightProperty = DependencyProperty.Register(
        nameof(ProgressBarHeight), typeof(double), typeof(ThemedProgressBar), new PropertyMetadata(5.0));

    public static readonly DependencyProperty HeaderLeftProperty = DependencyProperty.Register(
        nameof(HeaderLeft), typeof(string), typeof(ThemedProgressBar), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty HeaderRightProperty = DependencyProperty.Register(
        nameof(HeaderRight), typeof(string), typeof(ThemedProgressBar), new PropertyMetadata(default(string)));

    public string HeaderRight
    {
        get => (string)GetValue(HeaderRightProperty);
        set => SetValue(HeaderRightProperty, value);
    }

    public string HeaderLeft
    {
        get => (string)GetValue(HeaderLeftProperty);
        set => SetValue(HeaderLeftProperty, value);
    }

    public double ProgressBarHeight
    {
        get => (double)GetValue(ProgressBarHeightProperty);
        set => SetValue(ProgressBarHeightProperty, value);
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public ImageKey FinishedIcon
    {
        get => (ImageKey)GetValue(FinishedIconProperty);
        set => SetValue(FinishedIconProperty, value);
    }

    public string FooterMessage
    {
        get => (string)GetValue(FooterMessageProperty);
        set => SetValue(FooterMessageProperty, value);
    }

    static ThemedProgressBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ThemedProgressBar), new FrameworkPropertyMetadata(typeof(ThemedProgressBar)));
    }
}