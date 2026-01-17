using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ChemicalSimulator.Models;

namespace ChemicalSimulator.Views
{
    /// <summary>
    /// Converte PhysicalState para string em português
    /// </summary>
    public class PhysicalStateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PhysicalState state)
            {
                return state switch
                {
                    PhysicalState.Solid => "Sólido",
                    PhysicalState.Liquid => "Líquido",
                    PhysicalState.Gas => "Gasoso",
                    PhysicalState.Plasma => "Plasma",
                    _ => "Desconhecido"
                };
            }
            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte CompoundCategory para string em português
    /// </summary>
    public class CompoundCategoryToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CompoundCategory category)
            {
                return category switch
                {
                    CompoundCategory.Acid => "Ácido",
                    CompoundCategory.Base => "Base",
                    CompoundCategory.Salt => "Sal",
                    CompoundCategory.Oxide => "Óxido",
                    CompoundCategory.Organic => "Orgânico",
                    CompoundCategory.Inorganic => "Inorgânico",
                    CompoundCategory.HydroCarbon => "Hidrocarboneto",
                    CompoundCategory.Alcohol => "Álcool",
                    CompoundCategory.Polymer => "Polímero",
                    CompoundCategory.Other => "Outro",
                    _ => "Desconhecido"
                };
            }
            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte PhysicalState para cor
    /// </summary>
    public class PhysicalStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PhysicalState state)
            {
                return state switch
                {
                    PhysicalState.Solid => new SolidColorBrush(Color.FromRgb(139, 69, 19)),      // Marrom
                    PhysicalState.Liquid => new SolidColorBrush(Color.FromRgb(30, 144, 255)),    // Azul
                    PhysicalState.Gas => new SolidColorBrush(Color.FromRgb(144, 238, 144)),      // Verde claro
                    PhysicalState.Plasma => new SolidColorBrush(Color.FromRgb(255, 105, 180)),   // Rosa
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte CompoundCategory para cor
    /// </summary>
    public class CompoundCategoryToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CompoundCategory category)
            {
                return category switch
                {
                    CompoundCategory.Acid => new SolidColorBrush(Color.FromRgb(220, 20, 60)),        // Vermelho
                    CompoundCategory.Base => new SolidColorBrush(Color.FromRgb(70, 130, 180)),       // Azul aço
                    CompoundCategory.Salt => new SolidColorBrush(Color.FromRgb(255, 215, 0)),        // Dourado
                    CompoundCategory.Oxide => new SolidColorBrush(Color.FromRgb(255, 140, 0)),       // Laranja escuro
                    CompoundCategory.Organic => new SolidColorBrush(Color.FromRgb(34, 139, 34)),     // Verde floresta
                    CompoundCategory.Inorganic => new SolidColorBrush(Color.FromRgb(123, 104, 238)), // Roxo médio
                    CompoundCategory.HydroCarbon => new SolidColorBrush(Color.FromRgb(0, 100, 0)),   // Verde escuro
                    CompoundCategory.Alcohol => new SolidColorBrush(Color.FromRgb(147, 112, 219)),   // Roxo médio
                    CompoundCategory.Polymer => new SolidColorBrush(Color.FromRgb(199, 21, 133)),    // Magenta
                    CompoundCategory.Other => new SolidColorBrush(Color.FromRgb(128, 128, 128)),     // Cinza
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
