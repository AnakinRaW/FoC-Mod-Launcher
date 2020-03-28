using System;
using System.Globalization;
using System.Windows.Data;
using FocLauncherHost.Update.UpdateCatalog;

namespace FocLauncherHost.Converters
{
    internal class LauncherTypeNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is PreviewType previewType))
                return string.Empty;
            return previewType == PreviewType.Stable ? string.Empty : Enum.GetName(typeof(PreviewType), value)?.ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
