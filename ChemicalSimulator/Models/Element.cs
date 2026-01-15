using System;
using System.Windows.Media;

namespace ChemicalSimulator.Models
{
    /// <summary>
    /// Representa um elemento químico da tabela periódica
    /// </summary>
    public class Element
    {
        public int AtomicNumber { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public double AtomicMass { get; set; }
        public int Group { get; set; }
        public int Period { get; set; }
        public ElementCategory Category { get; set; }
        public double Electronegativity { get; set; }
        public int[] ValenceElectrons { get; set; }
        public double AtomicRadius { get; set; }
        public double IonizationEnergy { get; set; }

        // Configuração eletrônica
        public string ElectronConfiguration { get; set; }

        // Estados de oxidação comuns
        public int[] OxidationStates { get; set; }

        // Cor para visualização
        public Color DisplayColor { get; set; }

        // Raio de van der Waals (para renderização 3D)
        public double VanDerWaalsRadius { get; set; }

        public Element(int atomicNumber, string symbol, string name)
        {
            AtomicNumber = atomicNumber;
            Symbol = symbol;
            Name = name;
            DisplayColor = GetCategoryColor();
        }

        private Color GetCategoryColor()
        {
            return Category switch
            {
                ElementCategory.NonMetal => Color.FromRgb(255, 200, 200),
                ElementCategory.NobleGas => Color.FromRgb(200, 255, 255),
                ElementCategory.AlkaliMetal => Color.FromRgb(255, 150, 150),
                ElementCategory.AlkalineEarthMetal => Color.FromRgb(255, 220, 150),
                ElementCategory.TransitionMetal => Color.FromRgb(255, 192, 203),
                ElementCategory.Metalloid => Color.FromRgb(204, 204, 153),
                ElementCategory.Halogen => Color.FromRgb(255, 255, 153),
                _ => Color.FromRgb(192, 192, 192)
            };
        }
    }

    public enum ElementCategory
    {
        NonMetal,
        NobleGas,
        AlkaliMetal,
        AlkalineEarthMetal,
        TransitionMetal,
        PostTransitionMetal,
        Metalloid,
        Halogen,
        Lanthanide,
        Actinide
    }
}
