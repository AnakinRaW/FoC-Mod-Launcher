using System.Globalization;
using System.Windows;

namespace Sklavenwalker.CommonUtilities.Wpf.Converters;

internal class TreeViewItemIndentionConverter : MultiValueConverter<int, double, GridLength>
{
    protected override GridLength Convert(int depth, double indention, object parameter, CultureInfo culture)
    {
        return new GridLength(4 + depth * indention);
    }
}