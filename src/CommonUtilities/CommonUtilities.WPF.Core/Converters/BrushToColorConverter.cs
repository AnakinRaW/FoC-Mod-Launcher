using System.Globalization;
using System.Windows.Media;

namespace AnakinRaW.CommonUtilities.Wpf.Converters;

public class BrushToColorConverter : ValueConverter<Brush, Color>
{
    protected override Color Convert(Brush brush, object? parameter, CultureInfo culture)
    {
        return brush switch
        {
            SolidColorBrush solidColorBrush => solidColorBrush.Color,
            GradientBrush gradientBrush => gradientBrush.GradientStops.Count > 0
                ? gradientBrush.GradientStops[0].Color
                : Colors.Transparent,
            _ => Colors.Transparent
        };
    }
}