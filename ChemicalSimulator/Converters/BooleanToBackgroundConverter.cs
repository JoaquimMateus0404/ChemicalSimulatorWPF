using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ChemicalSimulator.Converters
{
    /// <summary>
    /// Converte bool para cor de fundo (validação visual)
    /// </summary>
    public class BooleanToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isValid)
            {
                return isValid 
                    ? new SolidColorBrush(Colors.Transparent) 
                    : new SolidColorBrush(Color.FromArgb(30, 255, 87, 34)); // Vermelho transparente
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
