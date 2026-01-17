using System.Globalization;
using System.Windows.Data;

namespace ChemicalSimulator.Converters
{
    /// <summary>
    /// Converte enum para string amigável
    /// </summary>
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            var enumString = value.ToString();

            // Adicionar espaços antes de letras maiúsculas
            var result = System.Text.RegularExpressions.Regex.Replace(
                enumString,
                "([A-Z])",
                " $1").Trim();

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}