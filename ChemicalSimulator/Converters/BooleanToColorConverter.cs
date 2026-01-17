using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ChemicalSimulator.Converters
{
    /// <summary>
    /// Converte booleano para cor
    /// </summary>
    public class BooleanToColorConverter : IValueConverter
    {
        public Brush TrueColor { get; set; } = Brushes.Green;
        public Brush FalseColor { get; set; } = Brushes.Red;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? TrueColor : FalseColor;
            return FalseColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}