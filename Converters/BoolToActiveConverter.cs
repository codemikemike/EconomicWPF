using System;
using System.Globalization;
using System.Windows.Data;

namespace EconomicWPF.Converters
{
    public class BoolToActiveConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "Aktiv" : "Inaktiv";
            }
            return "Inaktiv";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return stringValue.Equals("Aktiv", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}