using System.Globalization;
using System.Windows;
using AnakinRaW.CommonUtilities.Wpf.Converters;
using AnakinRaW.CommonUtilities.Wpf.Imaging.Controls;

namespace AnakinRaW.CommonUtilities.Wpf.Imaging.Converters;

public class ImageKeyToThemedImageConverter : ValueConverter<ImageKey, ThemedImage>
{
    protected override ThemedImage Convert(ImageKey source, object? parameter, CultureInfo culture)
    {
        return new ThemedImage { ImakgeKey = source };
    }
}

public class ImageKeyToVisibilityConverter : ValueConverter<ImageKey, Visibility>
{
    public Visibility HiddenState { get; set; } = Visibility.Collapsed;

    protected override Visibility Convert(ImageKey value, object? parameter, CultureInfo culture)
    {
        return value == default ? HiddenState : Visibility.Visible;
    }
}