using System.Globalization;
using System.Windows.Data;

namespace ChemicalSimulator.Converters
{
    /// <summary>
    /// Multiplica valor por um fator
    /// </summary>
    public class MultiplyConverter : IValueConverter
    {
        public double Factor { get; set; } = 1.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                var factor = parameter != null ? System.Convert.ToDouble(parameter) : Factor;
                return doubleValue * factor;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                var factor = parameter != null ? System.Convert.ToDouble(parameter) : Factor;
                return doubleValue / factor;
            }
            return 0.0;
        }
    }
}