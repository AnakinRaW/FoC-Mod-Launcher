using System.Globalization;
using System.Windows.Media;
using AnakinRaW.CommonUtilities.Wpf.Converters;

namespace AnakinRaW.CommonUtilities.Wpf.Imaging.Converters;

internal sealed class ActualGrayscaleBiasColorConverter : MultiValueConverter<Color?, bool, Color>
{
    public static readonly ActualGrayscaleBiasColorConverter Instance = new();

    protected override Color Convert(Color? grayscaleBiasColor, bool highContrast, object parameter, CultureInfo culture)
    {
        if (grayscaleBiasColor.HasValue)
            return grayscaleBiasColor.Value;
        return highContrast ? ImageLibrary.HighContrastGrayscaleBiasColor : ImageLibrary.DefaultGrayscaleBiasColor;
    }
}