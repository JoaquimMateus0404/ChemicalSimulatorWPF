using System;
using System.Collections.Generic;

namespace ChemicalSimulator.Services.Chemistry
{
    /// <summary>
    /// Tipos de reações químicas
    /// </summary>
    public enum ReactionType
    {
        Combustion,           // CxHy + O2 → CO2 + H2O
        Neutralization,       // Ácido + Base → Sal + H2O
        Synthesis,            // A + B → AB
        Decomposition,        // AB → A + B
        SingleDisplacement,   // A + BC → AC + B
        DoubleDisplacement,   // AB + CD → AD + CB
        Redox,                // Transferência de elétrons
        Addition,             // Dupla ligação + X2 → saturação
        Substitution,         // Troca de grupos funcionais
        NoReaction            // Reagentes incompatíveis
    }

    /// <summary>
    /// Representa uma regra química que pode ser aplicada
    /// </summary>
    public abstract class ChemicalRule
    {
        protected readonly ElementClassifier ElementClassifier;

        protected ChemicalRule(ElementClassifier elementClassifier)
        {
            ElementClassifier = elementClassifier;
        }

        /// <summary>
        /// Nome da regra
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de reação que a regra implementa
        /// </summary>
        public ReactionType Type { get; set; }

        /// <summary>
        /// Prioridade da regra (maior = mais prioritária)
        /// </summary>
        public int Priority { get; set; } = 50;

        /// <summary>
        /// Energia de ativação típica (kJ/mol)
        /// </summary>
        public double ActivationEnergy { get; set; }

        /// <summary>
        /// Variação de entalpia típica (kJ/mol)
        /// </summary>
        public double EnthalpyChange { get; set; }

        /// <summary>
        /// Verifica se a regra pode ser aplicada aos reagentes
        /// </summary>
        public abstract bool CanApply(List<MoleculeGraph> reactants, ReactionConditions? conditions = null);

        /// <summary>
        /// Aplica a regra e retorna os produtos
        /// </summary>
        public abstract List<MoleculeGraph> Apply(List<MoleculeGraph> reactants, ReactionConditions? conditions = null);

        /// <summary>
        /// Valida se os produtos obedecem conservação de massa/carga
        /// </summary>
        protected bool ValidateConservation(List<MoleculeGraph> reactants, List<MoleculeGraph> products)
        {
            // Conservação de átomos
            var reactantAtoms = GetAtomCounts(reactants);
            var productAtoms = GetAtomCounts(products);

            foreach (var element in reactantAtoms.Keys)
            {
                if (!productAtoms.ContainsKey(element) || reactantAtoms[element] != productAtoms[element])
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Conservação de massa violada: {element}");
                    return false;
                }
            }

            // Conservação de carga
            int reactantCharge = reactants.Sum(r => r.TotalCharge);
            int productCharge = products.Sum(p => p.TotalCharge);

            if (reactantCharge != productCharge)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Conservação de carga violada: {reactantCharge} ≠ {productCharge}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Conta quantos átomos de cada elemento existem
        /// </summary>
        protected Dictionary<string, int> GetAtomCounts(List<MoleculeGraph> molecules)
        {
            var counts = new Dictionary<string, int>();

            foreach (var molecule in molecules)
            {
                foreach (var atom in molecule.Atoms)
                {
                    if (!counts.ContainsKey(atom.Element))
                        counts[atom.Element] = 0;
                    counts[atom.Element]++;
                }
            }

            return counts;
        }
    }

    /// <summary>
    /// Condições da reação (temperatura, pressão, catalisador)
    /// </summary>
    public class ReactionConditions
    {
        public double Temperature { get; set; } = 298.15; // K (25°C padrão)
        public double Pressure { get; set; } = 101.325;   // kPa (1 atm)
        public string? Catalyst { get; set; }
        public bool HasLight { get; set; } = false;
        public bool HasElectricity { get; set; } = false;
    }
}
