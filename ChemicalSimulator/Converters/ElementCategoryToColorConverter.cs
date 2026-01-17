using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ChemicalSimulator.Converters
{
    /// <summary>
    /// Converte categoria de elemento para cor
    /// </summary>
    public class ElementCategoryToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.ElementCategory category)
            {
                return category switch
                {
                    Models.ElementCategory.NonMetal => new SolidColorBrush(Color.FromRgb(144, 238, 144)), // Verde claro
                    Models.ElementCategory.NobleGas => new SolidColorBrush(Color.FromRgb(135, 206, 250)), // Azul céu
                    Models.ElementCategory.AlkaliMetal => new SolidColorBrush(Color.FromRgb(255, 140, 140)), // Vermelho claro
                    Models.ElementCategory.AlkalineEarthMetal => new SolidColorBrush(Color.FromRgb(255, 218, 101)), // Amarelo dourado
                    Models.ElementCategory.TransitionMetal => new SolidColorBrush(Color.FromRgb(255, 182, 193)), // Rosa claro
                    Models.ElementCategory.PostTransitionMetal => new SolidColorBrush(Color.FromRgb(176, 196, 222)), // Azul aço claro
                    Models.ElementCategory.Metalloid => new SolidColorBrush(Color.FromRgb(238, 232, 170)), // Amarelo pálido
                    Models.ElementCategory.Halogen => new SolidColorBrush(Color.FromRgb(221, 160, 221)), // Lavanda
                    Models.ElementCategory.Lanthanide => new SolidColorBrush(Color.FromRgb(255, 192, 203)), // Rosa
                    Models.ElementCategory.Actinide => new SolidColorBrush(Color.FromRgb(233, 150, 122)), // Salmão
                    _ => new SolidColorBrush(Colors.LightGray)
                };
            }
            return new SolidColorBrush(Colors.LightGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}