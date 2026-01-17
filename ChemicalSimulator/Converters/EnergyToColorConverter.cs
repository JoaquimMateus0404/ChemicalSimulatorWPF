using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ChemicalSimulator.Converters
{
    /// <summary>
    /// Converte energia positiva/negativa para cor
    /// </summary>
    public class EnergyToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double energy)
            {
                if (energy < 0)
                    return new SolidColorBrush(Colors.Green); // Exotérmica
                else if (energy > 0)
                    return new SolidColorBrush(Colors.Red); // Endotérmica
                else
                    return new SolidColorBrush(Colors.Gray);
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}