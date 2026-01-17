using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using Newtonsoft.Json;
using ChemicalSimulator.Models;

namespace ChemicalSimulator.Services
{
    /// <summary>
    /// Carrega dados dos elementos químicos de arquivos JSON
    /// Implementa cache para performance e fallback para dados embarcados
    /// </summary>
    public class ElementDataLoader
    {
        private const string EMBEDDED_RESOURCE_NAME = "ChemicalSimulator.Resources.Data.ElementsData.json";
        private static List<Element> _cachedElements;
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Carrega todos os elementos químicos do JSON ou cache
        /// </summary>
        /// <param name="jsonFilePath">Caminho opcional do arquivo JSON. Se nulo, usa recurso embarcado</param>
        /// <returns>Lista de elementos químicos</returns>
        public List<Element> LoadElements(string jsonFilePath = null)
        {
            // Thread-safe double-checked locking
            if (_cachedElements != null)
                return _cachedElements;

            lock (_lockObject)
            {
                if (_cachedElements != null)
                    return _cachedElements;

                try
                {
                    string json = LoadJsonContent(jsonFilePath);
                    _cachedElements = ParseElementsFromJson(json);
                    EnrichElementsData(_cachedElements);
                    
                    return _cachedElements;
                }
                catch (Exception ex)
                {
                    // Log do erro e fallback para elementos básicos
                    System.Diagnostics.Debug.WriteLine($"⚠️ Erro ao carregar elementos do JSON: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"📌 Usando elementos básicos como fallback");
                    
                    _cachedElements = CreateBasicElements();
                    EnrichElementsData(_cachedElements);
                    
                    return _cachedElements;
                }
            }
        }

        /// <summary>
        /// Carrega o conteúdo JSON de arquivo ou recurso embarcado
        /// </summary>
        private string LoadJsonContent(string jsonFilePath)
        {
            // Se foi especificado um arquivo, tentar carregar dele
            if (!string.IsNullOrEmpty(jsonFilePath) && File.Exists(jsonFilePath))
            {
                System.Diagnostics.Debug.WriteLine($"📂 Carregando elementos de: {jsonFilePath}");
                return File.ReadAllText(jsonFilePath);
            }

            // Tentar carregar do recurso embarcado
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream(EMBEDDED_RESOURCE_NAME))
                {
                    if (stream != null)
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            System.Diagnostics.Debug.WriteLine($"📦 Carregando elementos do recurso embarcado");
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Não foi possível carregar recurso embarcado: {ex.Message}");
            }

            // Se não conseguiu carregar de nenhum lugar, tentar caminho relativo padrão
            string defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                "Resources", "Data", "ElementsData.json");
            
            if (File.Exists(defaultPath))
            {
                System.Diagnostics.Debug.WriteLine($"📂 Carregando elementos de: {defaultPath}");
                return File.ReadAllText(defaultPath);
            }

            throw new FileNotFoundException("Arquivo ElementsData.json não encontrado em nenhum local");
        }

        /// <summary>
        /// Faz o parse do JSON para objetos Element
        /// </summary>
        private List<Element> ParseElementsFromJson(string json)
        {
            var data = JsonConvert.DeserializeObject<ElementDataContainer>(json);
            
            if (data?.Elements == null || data.Elements.Count == 0)
            {
                throw new InvalidDataException("JSON não contém elementos válidos");
            }

            System.Diagnostics.Debug.WriteLine($"✅ {data.Elements.Count} elementos carregados do JSON");
            return data.Elements;
        }

        /// <summary>
        /// Enriquece os dados dos elementos com informações adicionais
        /// </summary>
        private void EnrichElementsData(List<Element> elements)
        {
            foreach (var element in elements)
            {
                // Configurar cor de exibição baseada na categoria
                element.DisplayColor = GetCategoryColor(element.Category);

                // Se não tiver raio de van der Waals, calcular estimativa
                if (element.VanDerWaalsRadius == 0)
                {
                    element.VanDerWaalsRadius = GetVanDerWaalsRadius(element.Symbol);
                }
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

        /// <summary>
        /// Obtém um elemento pelo símbolo químico
        /// </summary>
        public Element GetElementBySymbol(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return null;

            var elements = LoadElements();
            return elements.FirstOrDefault(e =>
                e.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Obtém um elemento pelo número atômico
        /// </summary>
        public Element GetElementByAtomicNumber(int atomicNumber)
        {
            if (atomicNumber <= 0)
                return null;

            var elements = LoadElements();
            return elements.FirstOrDefault(e => e.AtomicNumber == atomicNumber);
        }

        /// <summary>
        /// Obtém elementos pelo nome (busca parcial)
        /// </summary>
        public List<Element> SearchElementsByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new List<Element>();

            var elements = LoadElements();
            return elements.Where(e =>
                e.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Obtém todos os elementos de uma categoria específica
        /// </summary>
        public List<Element> GetElementsByCategory(ElementCategory category)
        {
            var elements = LoadElements();
            return elements.Where(e => e.Category == category).ToList();
        }

        /// <summary>
        /// Obtém elementos de um período específico
        /// </summary>
        public List<Element> GetElementsByPeriod(int period)
        {
            if (period <= 0 || period > 7)
                return new List<Element>();

            var elements = LoadElements();
            return elements.Where(e => e.Period == period).ToList();
        }

        /// <summary>
        /// Obtém elementos de um grupo específico
        /// </summary>
        public List<Element> GetElementsByGroup(int group)
        {
            if (group <= 0 || group > 18)
                return new List<Element>();

            var elements = LoadElements();
            return elements.Where(e => e.Group == group).ToList();
        }

        /// <summary>
        /// Limpa o cache de elementos (útil para testes ou recarregamento)
        /// </summary>
        public void ClearCache()
        {
            lock (_lockObject)
            {
                _cachedElements = null;
                System.Diagnostics.Debug.WriteLine("🔄 Cache de elementos limpo");
            }
        }

        /// <summary>
        /// Obtém estatísticas sobre os elementos carregados
        /// </summary>
        public ElementStatistics GetStatistics()
        {
            var elements = LoadElements();
            
            return new ElementStatistics
            {
                TotalElements = elements.Count,
                ElementsByCategory = elements.GroupBy(e => e.Category)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ElementsByPeriod = elements.GroupBy(e => e.Period)
                    .ToDictionary(g => g.Key, g => g.Count()),
                AverageAtomicMass = elements.Average(e => e.AtomicMass),
                MaxAtomicNumber = elements.Max(e => e.AtomicNumber)
            };
        }
    }

    /// <summary>
    /// Classe auxiliar para deserialização JSON
    /// </summary>
    internal class ElementDataContainer
    {
        [JsonProperty("elements")]
        public List<Element> Elements { get; set; }
    }

    /// <summary>
    /// Estatísticas sobre os elementos carregados
    /// </summary>
    public class ElementStatistics
    {
        public int TotalElements { get; set; }
        public Dictionary<ElementCategory, int> ElementsByCategory { get; set; }
        public Dictionary<int, int> ElementsByPeriod { get; set; }
        public double AverageAtomicMass { get; set; }
        public int MaxAtomicNumber { get; set; }

        public override string ToString()
        {
            return $"Total: {TotalElements} elementos | " +
                   $"Maior Z: {MaxAtomicNumber} | " +
                   $"Massa média: {AverageAtomicMass:F2}";
        }
    }
}