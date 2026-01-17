using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Newtonsoft.Json;
using ChemicalSimulator.Models;

namespace ChemicalSimulator.Services
{
    /// <summary>
    /// Carrega dados dos elementos químicos de arquivos JSON
    /// </summary>
    public class ElementDataLoader
    {
        private static List<Element> _cachedElements;

        public static List<Element> LoadElements(string jsonFilePath = null)
        {
            if (_cachedElements != null)
                return _cachedElements;

            try
            {
                string json;

                if (string.IsNullOrEmpty(jsonFilePath))
                {
                    // Usar dados embedded ou criar elementos básicos
                    _cachedElements = CreateBasicElements();
                }
                else
                {
                    json = File.ReadAllText(jsonFilePath);
                    var data = JsonConvert.DeserializeObject<ElementDataContainer>(json);
                    _cachedElements = data.Elements;

                    // Configurar cores
                    foreach (var element in _cachedElements)
                    {
                        element.DisplayColor = GetCategoryColor(element.Category);
                    }
                }

                return _cachedElements;
            }
            catch (Exception ex)
            {
                // Em caso de erro, retornar elementos básicos
                Console.WriteLine($"Erro ao carregar elementos: {ex.Message}");
                return CreateBasicElements();
            }
        }

        private static List<Element> CreateBasicElements()
        {
            var elements = new List<Element>();

            // Período 1
            elements.Add(CreateElement(1, "H", "Hidrogênio", 1.008, 1, 1, ElementCategory.NonMetal, 2.20));
            elements.Add(CreateElement(2, "He", "Hélio", 4.003, 18, 1, ElementCategory.NobleGas, 0));

            // Período 2
            elements.Add(CreateElement(3, "Li", "Lítio", 6.941, 1, 2, ElementCategory.AlkaliMetal, 0.98));
            elements.Add(CreateElement(4, "Be", "Berílio", 9.012, 2, 2, ElementCategory.AlkalineEarthMetal, 1.57));
            elements.Add(CreateElement(5, "B", "Boro", 10.81, 13, 2, ElementCategory.Metalloid, 2.04));
            elements.Add(CreateElement(6, "C", "Carbono", 12.011, 14, 2, ElementCategory.NonMetal, 2.55));
            elements.Add(CreateElement(7, "N", "Nitrogênio", 14.007, 15, 2, ElementCategory.NonMetal, 3.04));
            elements.Add(CreateElement(8, "O", "Oxigênio", 15.999, 16, 2, ElementCategory.NonMetal, 3.44));
            elements.Add(CreateElement(9, "F", "Flúor", 18.998, 17, 2, ElementCategory.Halogen, 3.98));
            elements.Add(CreateElement(10, "Ne", "Neônio", 20.180, 18, 2, ElementCategory.NobleGas, 0));

            // Período 3
            elements.Add(CreateElement(11, "Na", "Sódio", 22.990, 1, 3, ElementCategory.AlkaliMetal, 0.93));
            elements.Add(CreateElement(12, "Mg", "Magnésio", 24.305, 2, 3, ElementCategory.AlkalineEarthMetal, 1.31));
            elements.Add(CreateElement(13, "Al", "Alumínio", 26.982, 13, 3, ElementCategory.PostTransitionMetal, 1.61));
            elements.Add(CreateElement(14, "Si", "Silício", 28.086, 14, 3, ElementCategory.Metalloid, 1.90));
            elements.Add(CreateElement(15, "P", "Fósforo", 30.974, 15, 3, ElementCategory.NonMetal, 2.19));
            elements.Add(CreateElement(16, "S", "Enxofre", 32.065, 16, 3, ElementCategory.NonMetal, 2.58));
            elements.Add(CreateElement(17, "Cl", "Cloro", 35.453, 17, 3, ElementCategory.Halogen, 3.16));
            elements.Add(CreateElement(18, "Ar", "Argônio", 39.948, 18, 3, ElementCategory.NobleGas, 0));

            // Período 4 (seleção)
            elements.Add(CreateElement(19, "K", "Potássio", 39.098, 1, 4, ElementCategory.AlkaliMetal, 0.82));
            elements.Add(CreateElement(20, "Ca", "Cálcio", 40.078, 2, 4, ElementCategory.AlkalineEarthMetal, 1.00));

            // Metais de transição
            elements.Add(CreateElement(21, "Sc", "Escândio", 44.956, 3, 4, ElementCategory.TransitionMetal, 1.36));
            elements.Add(CreateElement(22, "Ti", "Titânio", 47.867, 4, 4, ElementCategory.TransitionMetal, 1.54));
            elements.Add(CreateElement(23, "V", "Vanádio", 50.942, 5, 4, ElementCategory.TransitionMetal, 1.63));
            elements.Add(CreateElement(24, "Cr", "Cromo", 51.996, 6, 4, ElementCategory.TransitionMetal, 1.66));
            elements.Add(CreateElement(25, "Mn", "Manganês", 54.938, 7, 4, ElementCategory.TransitionMetal, 1.55));
            elements.Add(CreateElement(26, "Fe", "Ferro", 55.845, 8, 4, ElementCategory.TransitionMetal, 1.83));
            elements.Add(CreateElement(27, "Co", "Cobalto", 58.933, 9, 4, ElementCategory.TransitionMetal, 1.88));
            elements.Add(CreateElement(28, "Ni", "Níquel", 58.693, 10, 4, ElementCategory.TransitionMetal, 1.91));
            elements.Add(CreateElement(29, "Cu", "Cobre", 63.546, 11, 4, ElementCategory.TransitionMetal, 1.90));
            elements.Add(CreateElement(30, "Zn", "Zinco", 65.38, 12, 4, ElementCategory.TransitionMetal, 1.65));

            elements.Add(CreateElement(35, "Br", "Bromo", 79.904, 17, 4, ElementCategory.Halogen, 2.96));
            elements.Add(CreateElement(36, "Kr", "Criptônio", 83.798, 18, 4, ElementCategory.NobleGas, 3.00));

            return elements;
        }

        private static Element CreateElement(int atomicNumber, string symbol, string name,
            double atomicMass, int group, int period, ElementCategory category, double electronegativity)
        {
            var element = new Element(atomicNumber, symbol, name)
            {
                AtomicMass = atomicMass,
                Group = group,
                Period = period,
                Category = category,
                Electronegativity = electronegativity,
                DisplayColor = GetCategoryColor(category),
                VanDerWaalsRadius = GetVanDerWaalsRadius(symbol)
            };

            return element;
        }

        private static Color GetCategoryColor(ElementCategory category)
        {
            return category switch
            {
                ElementCategory.NonMetal => Color.FromRgb(255, 228, 181),           // Pêssego claro
                ElementCategory.NobleGas => Color.FromRgb(192, 255, 255),           // Ciano claro
                ElementCategory.AlkaliMetal => Color.FromRgb(255, 107, 107),        // Vermelho claro
                ElementCategory.AlkalineEarthMetal => Color.FromRgb(255, 217, 61),  // Amarelo
                ElementCategory.TransitionMetal => Color.FromRgb(255, 192, 203),    // Rosa
                ElementCategory.PostTransitionMetal => Color.FromRgb(221, 221, 221),// Cinza claro
                ElementCategory.Metalloid => Color.FromRgb(204, 204, 153),          // Verde-amarelado
                ElementCategory.Halogen => Color.FromRgb(255, 255, 153),            // Amarelo claro
                ElementCategory.Lanthanide => Color.FromRgb(255, 191, 255),         // Rosa claro
                ElementCategory.Actinide => Color.FromRgb(255, 153, 204),           // Rosa médio
                _ => Color.FromRgb(192, 192, 192)                                   // Cinza
            };
        }

        private static double GetVanDerWaalsRadius(string symbol)
        {
            return symbol switch
            {
                "H" => 120,
                "C" => 170,
                "N" => 155,
                "O" => 152,
                "F" => 147,
                "P" => 180,
                "S" => 180,
                "Cl" => 175,
                "Br" => 185,
                "I" => 198,
                "Na" => 227,
                "Mg" => 173,
                "K" => 275,
                "Ca" => 231,
                "Fe" => 204,
                "Cu" => 196,
                "Zn" => 201,
                _ => 150
            };
        }

        public static Element GetElementBySymbol(string symbol)
        {
            var elements = LoadElements();
            return elements.FirstOrDefault(e =>
                e.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
        }

        public static Element GetElementByAtomicNumber(int atomicNumber)
        {
            var elements = LoadElements();
            return elements.FirstOrDefault(e => e.AtomicNumber == atomicNumber);
        }

        public static List<Element> GetElementsByCategory(ElementCategory category)
        {
            var elements = LoadElements();
            return elements.Where(e => e.Category == category).ToList();
        }
    }

    // Classe auxiliar para deserialização JSON
    internal class ElementDataContainer
    {
        [JsonProperty("elements")]
        public List<Element> Elements { get; set; }
    }
}