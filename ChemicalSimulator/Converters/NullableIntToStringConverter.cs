using System;
using System.Globalization;
using System.Windows.Data;

namespace ChemicalSimulator.Converters
{
    /// <summary>
    /// Converte int? para string, mostrando "-" ou "N/A" quando nulo
    /// </summary>
    public class NullableIntToStringConverter : IValueConverter
    {
        public string NullValue { get; set; } = "-";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue.ToString();
            }
            return NullValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && int.TryParse(str, out int result))
            {
                return result;
            }
            return null;
        }
    }
}
