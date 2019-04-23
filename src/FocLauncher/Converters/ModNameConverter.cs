using System;
using System.Globalization;
using System.Windows.Data;
using FocLauncher.Mods;

namespace FocLauncher.Converters
{
    public class ModNameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[1] == null)
                return "Forces of Corruption (unmodified)";
            if (values[1] is IMod mod)
            {
                if (!mod.WorkshopMod)
                    return mod.Name;
                return $"{mod.Name}*";
            }
            throw new ArgumentException("value has wrong type");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
