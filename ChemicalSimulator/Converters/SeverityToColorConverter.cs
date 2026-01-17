using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ChemicalSimulator.Converters
{
    /// <summary>
    /// Converte severidade de aviso para cor
    /// </summary>
    public class SeverityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string severity)
            {
                switch (severity.ToLower())
                {
                    case "error":
                    case "erro":
                        return new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Vermelho
                    case "warning":
                    case "aviso":
                        return new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Laranja
                    case "info":
                    case "informação":
                        return new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Azul
                    case "success":
                    case "sucesso":
                        return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Verde
                    default:
                        return new SolidColorBrush(Color.FromRgb(158, 158, 158)); // Cinza
                }
            }
            return new SolidColorBrush(Color.FromRgb(158, 158, 158));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
