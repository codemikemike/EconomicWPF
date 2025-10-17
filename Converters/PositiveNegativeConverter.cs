using System;
using System.Globalization;
using System.Windows.Data;

namespace EconomicWPF.Converters
{
    public class PositiveNegativeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "Zero";

            if (value is decimal decimalValue)
            {
                if (decimalValue > 0)
                    return "Positive";
                else if (decimalValue < 0)
                    return "Negative";
                else
                    return "Zero";
            }

            if (value is double doubleValue)
            {
                if (doubleValue > 0)
                    return "Positive";
                else if (doubleValue < 0)
                    return "Negative";
                else
                    return "Zero";
            }

            if (value is int intValue)
            {
                if (intValue > 0)
                    return "Positive";
                else if (intValue < 0)
                    return "Negative";
                else
                    return "Zero";
            }

            return "Zero";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}