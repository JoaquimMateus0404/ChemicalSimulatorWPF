using ChemicalSimulator.Models;

namespace ChemicalSimulator.Helpers
{
    /// <summary>
    /// Factory para criação de elementos químicos
    /// </summary>
    public static class ElementFactory
    {
        /// <summary>
        /// Cria um elemento químico com todas as propriedades
        /// </summary>
        public static Element CreateElement(
            int atomicNumber, 
            string symbol, 
            string name, 
            double atomicMass,
            double electronegativity = 0,
            ElementCategory? category = null)
        {
            return new Element(atomicNumber, symbol, name)
            {
                AtomicMass = atomicMass,
                Electronegativity = electronegativity,
                Category = category ?? DetermineCategory(atomicNumber)
            };
        }

        /// <summary>
        /// Determina a categoria de um elemento baseado no número atômico
        /// </summary>
        public static ElementCategory DetermineCategory(int atomicNumber)
        {
            return atomicNumber switch
            {
                1 => ElementCategory.NonMetal,
                2 => ElementCategory.NobleGas,
                >= 3 and <= 4 => ElementCategory.AlkaliMetal,
                >= 5 and <= 10 => ElementCategory.NonMetal,
                >= 11 and <= 12 => ElementCategory.AlkaliMetal,
                >= 13 and <= 18 => ElementCategory.NonMetal,
                >= 19 and <= 20 => ElementCategory.AlkaliMetal,
                >= 21 and <= 30 => ElementCategory.TransitionMetal,
                >= 31 and <= 36 => ElementCategory.NonMetal,
                >= 37 and <= 38 => ElementCategory.AlkaliMetal,
                >= 39 and <= 48 => ElementCategory.TransitionMetal,
                >= 49 and <= 54 => ElementCategory.NonMetal,
                >= 55 and <= 56 => ElementCategory.AlkaliMetal,
                >= 57 and <= 71 => ElementCategory.Lanthanide,
                >= 72 and <= 80 => ElementCategory.TransitionMetal,
                >= 81 and <= 86 => ElementCategory.NonMetal,
                >= 87 and <= 88 => ElementCategory.AlkaliMetal,
                >= 89 and <= 103 => ElementCategory.Actinide,
                _ => ElementCategory.NonMetal
            };
        }

        /// <summary>
        /// Cria uma molécula comum pré-definida
        /// </summary>
        public static Molecule CreateCommonMolecule(string name, string formula, double molarMass, double enthalpyOfFormation = 0)
        {
            return new Molecule
            {
                Name = name,
                Formula = formula,
                MolarMass = molarMass,
                EnthalpyOfFormation = enthalpyOfFormation
            };
        }
    }
}
