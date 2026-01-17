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
                    Models.ElementCategory.NonMetal => new SolidColorBrush(Color.FromRgb(255, 228, 181)),
                    Models.ElementCategory.NobleGas => new SolidColorBrush(Color.FromRgb(192, 255, 255)),
                    Models.ElementCategory.AlkaliMetal => new SolidColorBrush(Color.FromRgb(255, 107, 107)),
                    Models.ElementCategory.AlkalineEarthMetal => new SolidColorBrush(Color.FromRgb(255, 217, 61)),
                    Models.ElementCategory.TransitionMetal => new SolidColorBrush(Color.FromRgb(255, 192, 203)),
                    Models.ElementCategory.PostTransitionMetal => new SolidColorBrush(Color.FromRgb(221, 221, 221)),
                    Models.ElementCategory.Metalloid => new SolidColorBrush(Color.FromRgb(204, 204, 153)),
                    Models.ElementCategory.Halogen => new SolidColorBrush(Color.FromRgb(255, 255, 153)),
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