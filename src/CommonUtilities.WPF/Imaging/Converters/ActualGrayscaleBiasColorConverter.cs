using System.Globalization;
using System.Windows.Media;
using Sklavenwalker.CommonUtilities.Wpf.Converters;

namespace Sklavenwalker.CommonUtilities.Wpf.Imaging.Converters;

public sealed class ActualGrayscaleBiasColorConverter : MultiValueConverter<Color?, bool, Color>
{
    public static readonly ActualGrayscaleBiasColorConverter Instance = new();

    protected override Color Convert(Color? grayscaleBiasColor, bool highContrast, object parameter, CultureInfo culture)
    {
        if (grayscaleBiasColor.HasValue)
            return grayscaleBiasColor.Value;
        return highContrast ? ImageLibrary.HighContrastGrayscaleBiasColor : ImageLibrary.DefaultGrayscaleBiasColor;
    }
}