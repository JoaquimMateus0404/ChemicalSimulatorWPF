using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media.Media3D;

namespace ChemicalSimulator.Models
{
    /// <summary>
    /// Representa uma molécula completa com átomos e ligações
    /// </summary>
    public class Molecule
    {
        public string Name { get; set; }
        public string Formula { get; set; }
        public List<Atom> Atoms { get; set; }
        public List<Bond> Bonds { get; set; }
        public double MolarMass { get; set; }
        public MoleculeGeometry Geometry { get; set; }

        // Propriedades termodinâmicas
        public double EnthalpyOfFormation { get; set; } // kJ/mol
        public double EntropyOfFormation { get; set; } // J/(mol·K)

        // Propriedades físicas
        public double BoilingPoint { get; set; }
        public double MeltingPoint { get; set; }
        public bool IsPolar { get; set; }

        public Molecule()
        {
            Atoms = new List<Atom>();
            Bonds = new List<Bond>();
        }

        public void AddAtom(Element element, Point3D position)
        {
            var atom = new Atom
            {
                Element = element,
                Position = position,
                Id = Atoms.Count
            };
            Atoms.Add(atom);
        }

        public void AddBond(int atom1Id, int atom2Id, BondType bondType)
        {
            var bond = new Bond
            {
                Atom1 = Atoms[atom1Id],
                Atom2 = Atoms[atom2Id],
                Type = bondType,
                Length = CalculateBondLength(Atoms[atom1Id], Atoms[atom2Id])
            };
            Bonds.Add(bond);
        }

        private double CalculateBondLength(Atom a1, Atom a2)
        {
            var dx = a1.Position.X - a2.Position.X;
            var dy = a1.Position.Y - a2.Position.Y;
            var dz = a1.Position.Z - a2.Position.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public void CalculateMolarMass()
        {
            MolarMass = Atoms.Sum(atom => atom.Element.AtomicMass);
        }

        public string GetMolecularFormula()
        {
            var elementCounts = new Dictionary<string, int>();

            foreach (var atom in Atoms)
            {
                if (elementCounts.ContainsKey(atom.Element.Symbol))
                    elementCounts[atom.Element.Symbol]++;
                else
                    elementCounts[atom.Element.Symbol] = 1;
            }

            var formula = string.Join("", elementCounts
                .OrderBy(x => x.Key)
                .Select(x => x.Value > 1 ? $"{x.Key}{x.Value}" : x.Key));

            return formula;
        }

        public int GetTotalCharge()
        {
            return Atoms.Sum(a => a.Charge);
        }
    }

    /// <summary>
    /// Representa um átomo individual em uma molécula
    /// </summary>
    public class Atom
    {
        public int Id { get; set; }
        public Element Element { get; set; }
        public Point3D Position { get; set; }
        public int Charge { get; set; }
        public HybridizationType Hybridization { get; set; }
        public List<int> BondedAtomIds { get; set; }

        public Atom()
        {
            BondedAtomIds = new List<int>();
        }
    }

    public enum MoleculeGeometry
    {
        Linear,
        BentAngular,
        TrigonalPlanar,
        Tetrahedral,
        TrigonalBipyramidal,
        Octahedral,
        Seesaw,
        TPyramidal,
        SquarePlanar
    }

    public enum HybridizationType
    {
        SP,
        SP2,
        SP3,
        SP3D,
        SP3D2,
        None
    }
}