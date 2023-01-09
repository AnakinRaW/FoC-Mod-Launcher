using System.Globalization;
using System.Windows;

namespace AnakinRaW.CommonUtilities.Wpf.Converters;

public class TreeViewExpanderVisibilityConverter : MultiValueConverter<bool, bool, int, Visibility>
{
    protected override Visibility Convert(bool expandable, bool showRootExpander, int depth, object parameter, CultureInfo culture)
    {
        if (!expandable)
            return Visibility.Collapsed;
        if (depth != 0 || showRootExpander)
            return Visibility.Visible;
        return Visibility.Collapsed;
    }
}