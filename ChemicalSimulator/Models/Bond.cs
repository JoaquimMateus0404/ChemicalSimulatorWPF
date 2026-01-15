using System;
using System.Collections.Generic;

namespace ChemicalSimulator.Models
{
    /// <summary>
    /// Representa uma ligação química entre dois átomos
    /// </summary>
    public class Bond
    {
        public Atom Atom1 { get; set; }
        public Atom Atom2 { get; set; }
        public BondType Type { get; set; }
        public double Length { get; set; } // Angstroms
        public double Energy { get; set; } // kJ/mol
        public double Strength { get; set; }
        public bool IsPolar { get; set; }

        public int BondOrder => Type switch
        {
            BondType.Single => 1,
            BondType.Double => 2,
            BondType.Triple => 3,
            BondType.Aromatic => 1,
            _ => 0
        };

        public double GetBondEnergy()
        {
            // Energias de ligação típicas (kJ/mol)
            var e1 = Atom1.Element.Symbol;
            var e2 = Atom2.Element.Symbol;
            var key = $"{e1}-{e2}";

            var bondEnergies = new Dictionary<string, double>
            {
                {"C-C", 348}, {"C=C", 614}, {"C≡C", 839},
                {"C-H", 413}, {"O-H", 463}, {"N-H", 391},
                {"C-O", 358}, {"C=O", 799}, {"C-N", 293},
                {"N=N", 418}, {"N≡N", 945}, {"O=O", 498}
            };

            var bondKey = Type switch
            {
                BondType.Double => $"{e1}={e2}",
                BondType.Triple => $"{e1}≡{e2}",
                _ => key
            };

            return bondEnergies.ContainsKey(bondKey) ? bondEnergies[bondKey] : 300;
        }
    }

    public enum BondType
    {
        Single,
        Double,
        Triple,
        Ionic,
        Metallic,
        Hydrogen,
        Aromatic
    }

   
}