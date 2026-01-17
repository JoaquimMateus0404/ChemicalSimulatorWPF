using System.Globalization;
using System.Windows.Data;

namespace ChemicalSimulator.Converters
{
    /// <summary>
    /// Converte número para porcentagem
    /// </summary>
    public class NumberToPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
                return $"{doubleValue:P0}";
            return "0%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}