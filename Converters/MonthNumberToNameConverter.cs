using EconomicWPF.Converters;
using System;
using System.Globalization;
using System.Windows.Data;

namespace EconomicWPF.Converters
{
    public class MonthNumberToNameConverter : IValueConverter
    {
        private static readonly string[] MonthNames = new[]
        {
            "Januar", "Februar", "Marts", "April", "Maj", "Juni",
            "Juli", "August", "September", "Oktober", "November", "December"
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int monthNumber && monthNumber >= 1 && monthNumber <= 12)
            {
                return MonthNames[monthNumber - 1];
            }
            return "Ukendt";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string monthName)
            {
                for (int i = 0; i < MonthNames.Length; i++)
                {
                    if (MonthNames[i].Equals(monthName, StringComparison.OrdinalIgnoreCase))
                    {
                        return i + 1;
                    }
                }
            }
            return 1;
        }
    }
}