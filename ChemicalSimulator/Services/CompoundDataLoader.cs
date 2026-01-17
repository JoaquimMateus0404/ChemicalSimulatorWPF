using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using ChemicalSimulator.Models;

namespace ChemicalSimulator.Services
{
    /// <summary>
    /// Carrega dados de compostos químicos comuns de arquivos JSON
    /// </summary>
    public class CompoundDataLoader
    {
        private const string EMBEDDED_RESOURCE_NAME = "ChemicalSimulator.Resources.Data.CommonCompoundsData.json";
        private static List<Compound>? _cachedCompounds;
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Carrega todos os compostos químicos do JSON ou cache
        /// </summary>
        public List<Compound> LoadCompounds(string? jsonFilePath = null)
        {
            // Thread-safe double-checked locking
            if (_cachedCompounds != null)
                return _cachedCompounds;

            lock (_lockObject)
            {
                if (_cachedCompounds != null)
                    return _cachedCompounds;

                try
                {
                    string json = LoadJsonContent(jsonFilePath);
                    _cachedCompounds = ParseCompoundsFromJson(json);
                    EnrichCompoundsData(_cachedCompounds);
                    
                    return _cachedCompounds;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Erro ao carregar compostos do JSON: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"📌 Usando compostos básicos como fallback");
                    
                    _cachedCompounds = CreateBasicCompounds();
                    return _cachedCompounds;
                }
            }
        }

        private string LoadJsonContent(string? jsonFilePath)
        {
            // Lista de caminhos possíveis
            var possiblePaths = new List<string>
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Data", "CommonCompoundsData.json"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Resources", "Data", "CommonCompoundsData.json"),
                Path.Combine(Environment.CurrentDirectory, "Resources", "Data", "CommonCompoundsData.json")
            };

            if (!string.IsNullOrEmpty(jsonFilePath))
            {
                possiblePaths.Insert(0, jsonFilePath);
            }

            // Tentar cada caminho
            foreach (var path in possiblePaths)
            {
                var normalizedPath = Path.GetFullPath(path);
                if (File.Exists(normalizedPath))
                {
                    System.Diagnostics.Debug.WriteLine($"📂 Carregando compostos de: {normalizedPath}");
                    return File.ReadAllText(normalizedPath);
                }
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
                            System.Diagnostics.Debug.WriteLine($"✅ Carregando compostos do recurso embarcado");
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Erro ao carregar recurso embarcado: {ex.Message}");
            }

            throw new FileNotFoundException($"Arquivo CommonCompoundsData.json não encontrado");
        }

        private List<Compound> ParseCompoundsFromJson(string json)
        {
            var data = JsonConvert.DeserializeObject<CompoundDataContainer>(json);
            
            if (data?.Compounds == null || data.Compounds.Count == 0)
            {
                throw new InvalidDataException("JSON não contém compostos válidos");
            }

            System.Diagnostics.Debug.WriteLine($"✅ {data.Compounds.Count} compostos carregados do JSON");
            return data.Compounds;
        }

        private void EnrichCompoundsData(List<Compound> compounds)
        {
            foreach (var compound in compounds)
            {
                // Sempre determinar categoria baseada na fórmula (o JSON não tem este campo)
                compound.Category = DetermineCategory(compound.Formula, compound.Name);
            }
        }

        private CompoundCategory DetermineCategory(string formula, string name)
        {
            // Normalizar para comparação
            var nameLower = name.ToLower();
            
            // Ácidos (prioridade alta)
            if (nameLower.Contains("ácido") || nameLower.Contains("acido"))
                return CompoundCategory.Acid;
            
            // Bases
            if (nameLower.Contains("hidróxido") || nameLower.Contains("hidroxido") || 
                formula.EndsWith("OH") || formula.Contains("(OH)"))
                return CompoundCategory.Base;
            
            // Sais (compostos iônicos)
            if ((formula.Contains("Cl") || formula.Contains("Br") || formula.Contains("I")) && 
                (formula.Contains("Na") || formula.Contains("K") || formula.Contains("Ca") || formula.Contains("Mg")))
                return CompoundCategory.Salt;
            
            // Óxidos
            if ((formula.EndsWith("O") || formula.EndsWith("O2") || formula.EndsWith("O3") || 
                 formula.EndsWith("O4") || formula.Contains("O)")) && 
                !formula.Contains("H") && formula.Length <= 10)
                return CompoundCategory.Oxide;
            
            // Compostos orgânicos
            if (formula.Contains("C") && formula.Contains("H"))
            {
                // Álcoois (têm OH mas não COOH)
                if (formula.Contains("OH") && !formula.Contains("COOH"))
                    return CompoundCategory.Alcohol;
                
                // Hidrocarbonetos (apenas C e H)
                if (!formula.Contains("O") && !formula.Contains("N") && !formula.Contains("S"))
                    return CompoundCategory.HydroCarbon;
                
                // Outros orgânicos
                return CompoundCategory.Organic;
            }
            
            // Padrão: Inorgânico
            return CompoundCategory.Inorganic;
        }

        private List<Compound> CreateBasicCompounds()
        {
            return new List<Compound>
            {
                new Compound("Água", "H2O", 18.015) 
                { 
                    State = PhysicalState.Liquid, 
                    BoilingPoint = 373.15, 
                    MeltingPoint = 273.15,
                    CommonUse = "Solvente universal",
                    Category = CompoundCategory.Inorganic
                },
                new Compound("Dióxido de Carbono", "CO2", 44.01) 
                { 
                    State = PhysicalState.Gas, 
                    BoilingPoint = 194.65,
                    CommonUse = "Refrigerantes, extintor de incêndios",
                    Category = CompoundCategory.Oxide
                },
                new Compound("Cloreto de Sódio", "NaCl", 58.44) 
                { 
                    State = PhysicalState.Solid, 
                    BoilingPoint = 1738.15,
                    MeltingPoint = 1074.15,
                    CommonUse = "Sal de cozinha",
                    Category = CompoundCategory.Salt
                },
                new Compound("Amônia", "NH3", 17.03) 
                { 
                    State = PhysicalState.Gas, 
                    BoilingPoint = 239.81,
                    CommonUse = "Fertilizantes, produtos de limpeza",
                    Category = CompoundCategory.Base
                },
                new Compound("Metano", "CH4", 16.04) 
                { 
                    State = PhysicalState.Gas, 
                    BoilingPoint = 111.65,
                    CommonUse = "Gás natural, combustível",
                    Category = CompoundCategory.HydroCarbon
                }
            };
        }

        /// <summary>
        /// Busca compostos por nome ou fórmula
        /// </summary>
        public List<Compound> SearchCompounds(string searchText)
        {
            var compounds = LoadCompounds();
            
            if (string.IsNullOrWhiteSpace(searchText))
                return compounds;
            
            return compounds.Where(c => 
                c.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                c.Formula.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                (c.CommonUse?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
            ).ToList();
        }

        /// <summary>
        /// Obtém compostos por categoria
        /// </summary>
        public List<Compound> GetCompoundsByCategory(CompoundCategory category)
        {
            var compounds = LoadCompounds();
            return compounds.Where(c => c.Category == category).ToList();
        }

        /// <summary>
        /// Obtém compostos por estado físico
        /// </summary>
        public List<Compound> GetCompoundsByState(PhysicalState state)
        {
            var compounds = LoadCompounds();
            return compounds.Where(c => c.State == state).ToList();
        }

        /// <summary>
        /// Limpa o cache de compostos
        /// </summary>
        public void ClearCache()
        {
            lock (_lockObject)
            {
                _cachedCompounds = null;
            }
        }
    }

    /// <summary>
    /// Classe auxiliar para deserialização JSON
    /// </summary>
    internal class CompoundDataContainer
    {
        [JsonProperty("compounds")]
        public List<Compound> Compounds { get; set; } = new List<Compound>();
    }
}
