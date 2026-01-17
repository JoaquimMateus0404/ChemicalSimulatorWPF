using System;
using System.Globalization;
using System.Windows.Data;
using ChemicalSimulator.Models;

namespace ChemicalSimulator.Converters
{
    /// <summary>
    /// Converte a posição do elemento (grupo e período) para coordenadas Canvas
    /// </summary>
    public class ElementPositionConverter : IMultiValueConverter
    {
        private const double ElementWidth = 74;  // Largura do elemento incluindo margem
        private const double ElementHeight = 84; // Altura do elemento incluindo margem

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 4 || values[2] == null || values[3] == null)
                return 0;

            // Group pode ser null para lantanídeos/actinídeos
            int? groupNullable = values[0] as int?;
            int group = groupNullable ?? 0;
            
            int period = System.Convert.ToInt32(values[1]);
            ElementCategory category = (ElementCategory)values[2];
            int atomicNumber = System.Convert.ToInt32(values[3]);

            string direction = parameter?.ToString() ?? "Left";

            // Determinar posicionamento especial para lantanídeos e actinídeos
            if (category == ElementCategory.Lanthanide)
            {
                period = 9;  // Linha separada para lantanídeos
                // Calcular posição baseada no número atômico
                // Lantanídeos: Ce(58) até Lu(71) - 14 elementos
                group = (atomicNumber - 57); // Ce=1, Pr=2, ..., Lu=14
            }
            else if (category == ElementCategory.Actinide)
            {
                period = 10; // Linha separada para actinídeos
                // Actinídeos: Th(90) até Lr(103) - 14 elementos
                group = (atomicNumber - 89); // Th=1, Pa=2, ..., Lr=14
            }
            else if (group == 0)
            {
                // Fallback: se não tiver grupo definido, tentar posicionar baseado no número atômico
                System.Diagnostics.Debug.WriteLine($"⚠️ Elemento {atomicNumber} sem grupo definido");
                return 0;
            }

            if (direction == "Left")
            {
                // Coluna (grupo)
                double columnOffset = (group - 1) * ElementWidth;
                
                // Para lantanídeos e actinídeos, adicionar offset para centralizá-los
                if (category == ElementCategory.Lanthanide || category == ElementCategory.Actinide)
                {
                    columnOffset += ElementWidth * 2; // Deslocar para direita
                }
                
                return columnOffset;
            }
            else // "Top"
            {
                // Linha (período)
                double rowOffset = (period - 1) * ElementHeight;
                
                // Adicionar espaço extra antes das linhas de lantanídeos/actinídeos
                if (period >= 9)
                {
                    rowOffset += 30; // Espaço de separação visual
                }
                
                return rowOffset;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
