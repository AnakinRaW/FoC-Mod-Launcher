using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace FocLauncherApp.ExceptionHandling
{
    internal class StackTraceToShortMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? string.Empty : GetFirstTwoLines(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static string GetFirstTwoLines(string input)
        {
            string[] stringSeparators = { "\r\n" };
            var lines = input.Split(stringSeparators, StringSplitOptions.None).ToList();

            var count = 0;
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                if (count == 2)
                {
                    sb.AppendLine("...");
                    break;
                }
                sb.AppendLine(line);
                ++count;
            }

            return sb.ToString();
        }
    }
}
