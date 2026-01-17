using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ChemicalSimulator.Converters
{
    /// <summary>
    /// Converte o texto de dificuldade (Básico/Intermediário/Avançado) em uma cor de "chip".
    /// </summary>
    public sealed class DifficultyToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var difficulty = (value as string)?.Trim() ?? string.Empty;

            return difficulty switch
            {
                "Básico" => new SolidColorBrush(Color.FromRgb(76, 175, 80)),          // #4CAF50
                "Intermediário" => new SolidColorBrush(Color.FromRgb(33, 150, 243)), // #2196F3
                "Avançado" => new SolidColorBrush(Color.FromRgb(255, 87, 34)),       // #FF5722
                _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))                // #9E9E9E
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }

    /// <summary>
    /// Converte o tipo de conteúdo em um ícone MaterialDesign
    /// </summary>
    public sealed class ContentTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var contentType = (value as string)?.Trim() ?? string.Empty;

            return contentType switch
            {
                "Teoria" => "BookOpenPageVariant",
                "Exercício" => "Pencil",
                "Prática" => "FlaskOutline",
                "Quiz" => "HelpCircleOutline",
                "Vídeo" => "PlayCircleOutline",
                _ => "BookEducationOutline"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
