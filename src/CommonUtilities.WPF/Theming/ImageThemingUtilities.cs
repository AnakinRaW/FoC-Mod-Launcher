using System;
using System.Windows;
using System.Windows.Media;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.Theming;

public static class ImageThemingUtilities
{
    public static event EventHandler<DependencyPropertyChangedEventArgs>? ThemeScrollBarsChanged;

    public static readonly DependencyProperty ImageBackgroundColorProperty = DependencyProperty.RegisterAttached(
        "ImageBackgroundColor", typeof(Color), typeof(ImageThemingUtilities),
        new FrameworkPropertyMetadata(Colors.Transparent, FrameworkPropertyMetadataOptions.Inherits));

    public static readonly DependencyProperty ThemeScrollBarsProperty = DependencyProperty.RegisterAttached(
        "ThemeScrollBars", typeof(bool?), typeof(ImageThemingUtilities),
        new FrameworkPropertyMetadata(null, OnThemeScrollBarsChanged));

    public static bool IsImageThemingEnabled { get; set; }
        
    static ImageThemingUtilities()
    {
        IsImageThemingEnabled = true;
    }

    public static Color GetImageBackgroundColor(DependencyObject obj) => (Color)obj.GetValue(ImageBackgroundColorProperty);

    public static void SetImageBackgroundColor(DependencyObject obj, Color value) => obj.SetValue(ImageBackgroundColorProperty, value);

    public static bool? GetThemeScrollBars(DependencyObject element)
    {
        Requires.NotNull((object)element, nameof(element));
        return (bool?)element.GetValue(ThemeScrollBarsProperty);
    }

    public static void SetThemeScrollBars(DependencyObject element, bool? value)
    {
        Requires.NotNull((object)element, nameof(element));
        element.SetValue(ThemeScrollBarsProperty, value);
    }

    // TODO
    public static bool IsDark(this Color color) => false;

    public static bool IsLight(this Color color)
    {
        return !color.IsDark();
    }

    private static void OnThemeScrollBarsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ThemeScrollBarsChanged?.Invoke(d, e);
    }
}