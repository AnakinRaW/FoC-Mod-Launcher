using System.Globalization;
using AnakinRaW.CommonUtilities.Wpf.Converters;

namespace AnakinRaW.CommonUtilities.Wpf.Imaging.Converters;

internal sealed class ActualHighContrastConverter : MultiValueConverter<bool?, bool, bool>
{
    public static readonly ActualHighContrastConverter Instance = new();

    protected override bool Convert(bool? highContrast, bool systemHighContrast, object parameter, CultureInfo culture)
    {
        return highContrast.HasValue ? highContrast.Value : systemHighContrast;
    }
}