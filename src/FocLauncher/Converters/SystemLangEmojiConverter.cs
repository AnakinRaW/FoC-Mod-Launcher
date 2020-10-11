using System;
using System.Globalization;
using System.Windows.Data;

namespace FocLauncher.Converters
{
    internal class SystemLangEmojiConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            CultureInfo ci = CultureInfo.InstalledUICulture;

            var i = ci.NativeName.IndexOf('(');
            string nameWithoutCountry = ci.NativeName;
            if (i > -1)
                nameWithoutCountry = ci.NativeName.Substring(0, i);
            return $"{parameter}{nameWithoutCountry}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
