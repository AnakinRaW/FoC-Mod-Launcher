using System;
using System.Globalization;
using System.Windows.Data;
using FocLauncherHost.Updater.Restart;

namespace FocLauncherHost.Utilities
{
    internal class LockingProcessInfoTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ILockingProcessInfo lockingProcessInfo))
                throw new NotSupportedException();

            return $"{lockingProcessInfo.Description}.exe [{lockingProcessInfo.Id}]";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
