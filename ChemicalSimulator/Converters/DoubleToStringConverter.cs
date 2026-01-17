using System;
using System.Globalization;
using System.Windows.Data;

namespace ChemicalSimulator.Converters
{
    /// <summary>
    /// Converte valor double para string formatada
    /// </summary>
    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                var format = parameter as string ?? "F2";
                return doubleValue.ToString(format);
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse(value?.ToString(), out double result))
                return result;
            return 0.0;
        }
    }
}