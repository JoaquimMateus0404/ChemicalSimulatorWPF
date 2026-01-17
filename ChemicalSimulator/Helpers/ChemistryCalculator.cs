using System;
using System.Windows.Media.Media3D;
using ChemicalSimulator.Models;

namespace ChemicalSimulator.Helpers
{
    /// <summary>
    /// Classe auxiliar para cálculos químicos comuns
    /// </summary>
    public static class ChemistryCalculator
    {
        /// <summary>
        /// Calcula a distância entre dois pontos 3D
        /// </summary>
        public static double CalculateDistance(Point3D point1, Point3D point2)
        {
            var dx = point1.X - point2.X;
            var dy = point1.Y - point2.Y;
            var dz = point1.Z - point2.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// Calcula o comprimento de ligação entre dois átomos
        /// </summary>
        public static double CalculateBondLength(Atom atom1, Atom atom2)
        {
            return CalculateDistance(atom1.Position, atom2.Position);
        }

        /// <summary>
        /// Retorna o comprimento ideal de ligação baseado no tipo e elementos
        /// </summary>
        public static double GetIdealBondLength(BondType bondType, Element element1, Element element2)
        {
            // Raios covalentes aproximados (em Angstroms)
            var covalentRadii = new System.Collections.Generic.Dictionary<string, double>
            {
                {"H", 0.31}, {"C", 0.76}, {"N", 0.71}, {"O", 0.66},
                {"F", 0.57}, {"P", 1.07}, {"S", 1.05}, {"Cl", 0.99},
                {"Br", 1.20}, {"I", 1.39}
            };

            double radius1 = covalentRadii.ContainsKey(element1.Symbol) 
                ? covalentRadii[element1.Symbol] : 1.0;
            double radius2 = covalentRadii.ContainsKey(element2.Symbol) 
                ? covalentRadii[element2.Symbol] : 1.0;

            double baseLength = radius1 + radius2;

            return bondType switch
            {
                BondType.Single => baseLength,
                BondType.Double => baseLength * 0.87,
                BondType.Triple => baseLength * 0.78,
                BondType.Aromatic => baseLength * 0.93,
                _ => baseLength
            };
        }

        /// <summary>
        /// Calcula a energia de uma ligação química
        /// </summary>
        public static double CalculateBondEnergy(Bond bond)
        {
            var bondEnergies = new System.Collections.Generic.Dictionary<string, double>
            {
                {"H-H", 436}, {"C-H", 413}, {"N-H", 391}, {"O-H", 463},
                {"C-C", 348}, {"C=C", 614}, {"C≡C", 839},
                {"C-O", 358}, {"C=O", 799}, {"C-N", 293},
                {"N-N", 163}, {"N=N", 418}, {"N≡N", 945},
                {"O-O", 146}, {"O=O", 498}, {"F-F", 158},
                {"Cl-Cl", 243}, {"Br-Br", 193}, {"I-I", 151}
            };

            string symbol1 = bond.Atom1.Element.Symbol;
            string symbol2 = bond.Atom2.Element.Symbol;
            
            string separator = bond.Type switch
            {
                BondType.Double => "=",
                BondType.Triple => "≡",
                _ => "-"
            };

            string key = $"{symbol1}{separator}{symbol2}";
            string reverseKey = $"{symbol2}{separator}{symbol1}";

            if (bondEnergies.ContainsKey(key))
                return bondEnergies[key];
            if (bondEnergies.ContainsKey(reverseKey))
                return bondEnergies[reverseKey];

            // Energia padrão se não encontrar na tabela
            return 300.0;
        }

        /// <summary>
        /// Calcula a diferença de eletronegatividade entre dois elementos
        /// </summary>
        public static double CalculateElectronegativityDifference(Element element1, Element element2)
        {
            return Math.Abs(element1.Electronegativity - element2.Electronegativity);
        }

        /// <summary>
        /// Determina o tipo de ligação baseado na diferença de eletronegatividade
        /// </summary>
        public static BondType DetermineBondType(Element element1, Element element2)
        {
            double diff = CalculateElectronegativityDifference(element1, element2);

            if (diff > 1.7)
                return BondType.Ionic;
            else if (diff > 0.4)
                return BondType.Single; // Covalente polar
            else
                return BondType.Single; // Covalente apolar
        }

        /// <summary>
        /// Calcula a massa molar de uma molécula
        /// </summary>
        public static double CalculateMolarMass(Molecule molecule)
        {
            double mass = 0;
            foreach (var atom in molecule.Atoms)
            {
                mass += atom.Element.AtomicMass;
            }
            return mass;
        }

        /// <summary>
        /// Gera a fórmula molecular de uma molécula
        /// </summary>
        public static string GenerateMolecularFormula(Molecule molecule)
        {
            var elementCounts = new System.Collections.Generic.Dictionary<string, int>();

            foreach (var atom in molecule.Atoms)
            {
                if (elementCounts.ContainsKey(atom.Element.Symbol))
                    elementCounts[atom.Element.Symbol]++;
                else
                    elementCounts[atom.Element.Symbol] = 1;
            }

            var formula = string.Join("", 
                System.Linq.Enumerable.OrderBy(elementCounts, x => x.Key)
                    .Select(x => x.Value > 1 ? $"{x.Key}{x.Value}" : x.Key));

            return string.IsNullOrEmpty(formula) ? "Unknown" : formula;
        }
    }
}
