using System.Globalization;
using System.Windows;

namespace AnakinRaW.CommonUtilities.Wpf.Converters;

public class TreeViewItemIndentionConverter : MultiValueConverter<int, double, GridLength>
{
    protected override GridLength Convert(int depth, double indention, object parameter, CultureInfo culture)
    {
        return new GridLength(4 + depth * indention);
    }
}