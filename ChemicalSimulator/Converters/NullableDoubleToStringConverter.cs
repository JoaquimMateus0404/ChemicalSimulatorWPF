using System;
using System.Globalization;
using System.Windows.Data;

namespace ChemicalSimulator.Converters
{
    /// <summary>
    /// Converte double? para string formatada, mostrando "-" quando nulo
    /// </summary>
    public class NullableDoubleToStringConverter : IValueConverter
    {
        public string NullValue { get; set; } = "-";
        public string Format { get; set; } = "F2"; // Formato padrão: 2 casas decimais

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // O parâmetro pode sobrescrever o formato
            string format = parameter as string ?? Format;

            if (value is double doubleValue)
            {
                return doubleValue.ToString(format, culture);
            }
            return NullValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && double.TryParse(str, out double result))
            {
                return result;
            }
            return null;
        }
    }
}
