using System;
using System.Globalization;
using System.Windows.Data;

namespace FocLauncherHost.ExceptionHandling
{
    internal class ExceptionToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Exception exception))
                return "Error could not be determined";

            return $"{exception.GetType().Name}: {exception.Message}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
