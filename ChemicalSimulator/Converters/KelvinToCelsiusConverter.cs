using System.Globalization;
using System.Windows.Data;

namespace ChemicalSimulator.Converters
{
    /// <summary>
    /// Converte temperatura Kelvin para Celsius
    /// </summary>
    public class KelvinToCelsiusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double kelvin)
            {
                var celsius = kelvin - 273.15;
                return $"{celsius:F1} °C";
            }
            return "0 °C";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}