using System;
using Newtonsoft.Json;

namespace ChemicalSimulator.Models
{
    /// <summary>
    /// Representa um composto químico comum
    /// </summary>
    public class Compound
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("formula")]
        public string Formula { get; set; } = string.Empty;
        
        [JsonProperty("molarMass")]
        public double MolarMass { get; set; }
        
        [JsonProperty("state")]
        public PhysicalState State { get; set; }
        
        [JsonProperty("boilingPoint")]
        public double? BoilingPoint { get; set; }  // Em Kelvin
        
        [JsonProperty("meltingPoint")]
        public double? MeltingPoint { get; set; }   // Em Kelvin
        
        [JsonProperty("commonUse")]
        public string? CommonUse { get; set; }
        
        [JsonProperty("category")]
        public CompoundCategory Category { get; set; }

        public Compound()
        {
        }

        public Compound(string name, string formula, double molarMass)
        {
            Name = name;
            Formula = formula;
            MolarMass = molarMass;
        }

        /// <summary>
        /// Retorna o ponto de ebulição em Celsius
        /// </summary>
        public double? BoilingPointCelsius => BoilingPoint.HasValue ? BoilingPoint.Value - 273.15 : null;

        /// <summary>
        /// Retorna o ponto de fusão em Celsius
        /// </summary>
        public double? MeltingPointCelsius => MeltingPoint.HasValue ? MeltingPoint.Value - 273.15 : null;

        public override string ToString()
        {
            return $"{Name} ({Formula})";
        }
    }

    /// <summary>
    /// Estado físico do composto
    /// </summary>
    public enum PhysicalState
    {
        Solid,
        Liquid,
        Gas,
        Aqueous,
        Plasma
    }

    /// <summary>
    /// Categoria do composto químico
    /// </summary>
    public enum CompoundCategory
    {
        Acid,           // Ácidos
        Base,           // Bases
        Salt,           // Sais
        Oxide,          // Óxidos
        Organic,        // Compostos Orgânicos
        Inorganic,      // Compostos Inorgânicos
        HydroCarbon,    // Hidrocarbonetos
        Alcohol,        // Álcoois
        Polymer,        // Polímeros
        Other           // Outros
    }
}
