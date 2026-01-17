using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ChemicalSimulator.Converters
{
    /// <summary>
    /// Converte valor nulo para visibilidade
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public Visibility NullVisibility { get; set; } = Visibility.Collapsed;
        public Visibility NotNullVisibility { get; set; } = Visibility.Visible;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? NullVisibility : NotNullVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}