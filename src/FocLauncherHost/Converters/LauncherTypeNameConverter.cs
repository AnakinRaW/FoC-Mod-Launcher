using System;
using System.Globalization;
using System.Windows.Data;
using FocLauncher;

namespace FocLauncherHost.Converters
{
    internal class LauncherTypeNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ApplicationType previewType))
                return string.Empty;
            return previewType == ApplicationType.Stable ? string.Empty : Enum.GetName(typeof(ApplicationType), value)?.ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
