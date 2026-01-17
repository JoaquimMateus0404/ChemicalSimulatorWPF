using System;
using System.Collections.Generic;
using System.Linq;
using ChemicalSimulator.Models;

namespace ChemicalSimulator.Services
{
    /// <summary>
    /// Motor de cálculos químicos
    /// </summary>
    public class ChemistryEngine
    {
        public double CalculateBondEnergy(Bond bond)
        {
            // Energias médias de ligação em kJ/mol
            var bondEnergies = new Dictionary<string, double>
            {
                {"H-H", 436}, {"C-H", 413}, {"N-H", 391}, {"O-H", 463},
                {"C-C", 348}, {"C=C", 614}, {"C≡C", 839},
                {"C-O", 358}, {"C=O", 799}, {"C-N", 293},
                {"N-N", 163}, {"N=N", 418}, {"N≡N", 945},
                {"O-O", 146}, {"O=O", 498}
            };

            var key = $"{bond.Atom1.Element.Symbol}-{bond.Atom2.Element.Symbol}";

            if (bond.Type == BondType.Double)
                key = $"{bond.Atom1.Element.Symbol}={bond.Atom2.Element.Symbol}";
            else if (bond.Type == BondType.Triple)
                key = $"{bond.Atom1.Element.Symbol}≡{bond.Atom2.Element.Symbol}";

            return bondEnergies.ContainsKey(key) ? bondEnergies[key] : 300;
        }

        public double CalculateElectronegativityDifference(Element e1, Element e2)
        {
            double electronegativity1 = e1.Electronegativity ?? 0;
            double electronegativity2 = e2.Electronegativity ?? 0;
            return Math.Abs(electronegativity1 - electronegativity2);
        }

        public BondType DetermineBondType(Element e1, Element e2)
        {
            var diff = CalculateElectronegativityDifference(e1, e2);

            if (diff > 1.7)
                return BondType.Ionic;
            else if (diff > 0.4)
                return BondType.Single; // Covalente polar
            else
                return BondType.Single; // Covalente apolar
        }

        public MoleculeGeometry DetermineMolecularGeometry(Molecule molecule)
        {
            if (molecule.Atoms.Count == 2)
                return MoleculeGeometry.Linear;

            // VSEPR - Teoria da repulsão dos pares de elétrons
            var centralAtom = molecule.Atoms
                .OrderByDescending(a => a.BondedAtomIds.Count)
                .FirstOrDefault();

            if (centralAtom == null)
                return MoleculeGeometry.Linear;

            int bondedAtoms = centralAtom.BondedAtomIds.Count;
            int lonePairs = (centralAtom.Element.ValenceElectrons?.Sum() ?? 0 - bondedAtoms) / 2;

            return (bondedAtoms, lonePairs) switch
            {
                (2, 0) => MoleculeGeometry.Linear,
                (2, 1) or (2, 2) => MoleculeGeometry.BentAngular,
                (3, 0) => MoleculeGeometry.TrigonalPlanar,
                (3, 1) => MoleculeGeometry.TPyramidal,
                (4, 0) => MoleculeGeometry.Tetrahedral,
                (5, 0) => MoleculeGeometry.TrigonalBipyramidal,
                (6, 0) => MoleculeGeometry.Octahedral,
                _ => MoleculeGeometry.Linear
            };
        }

        public double CalculateDipoleMoment(Molecule molecule)
        {
            // Cálculo simplificado do momento dipolar
            double totalDipole = 0;

            foreach (var bond in molecule.Bonds)
            {
                var diff = CalculateElectronegativityDifference(
                    bond.Atom1.Element,
                    bond.Atom2.Element);

                totalDipole += diff * bond.Length;
            }

            return totalDipole;
        }

        public bool IsPolar(Molecule molecule)
        {
            return CalculateDipoleMoment(molecule) > 0.1;
        }
    }
}