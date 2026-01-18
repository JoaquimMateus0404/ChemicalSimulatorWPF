using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChemicalSimulator.Services.Chemistry
{
    /// <summary>
    /// Representação de uma molécula como grafo
    /// Nós = Átomos | Arestas = Ligações
    /// </summary>
    public class MoleculeGraph
    {
        /// <summary>
        /// Átomos na molécula (nós do grafo)
        /// </summary>
        public List<Atom> Atoms { get; set; } = new List<Atom>();

        /// <summary>
        /// Ligações na molécula (arestas do grafo)
        /// </summary>
        public List<ChemicalBond> Bonds { get; set; } = new List<ChemicalBond>();

        /// <summary>
        /// Fórmula molecular (ex: H2O, CH4)
        /// </summary>
        public string Formula { get; set; } = string.Empty;

        /// <summary>
        /// Nome da molécula
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Carga total da molécula
        /// </summary>
        public int TotalCharge => Atoms.Sum(a => a.Charge);

        /// <summary>
        /// Adiciona um átomo ao grafo
        /// </summary>
        public Atom AddAtom(string element)
        {
            var atom = new Atom(element);
            Atoms.Add(atom);
            return atom;
        }

        /// <summary>
        /// Adiciona uma ligação entre dois átomos
        /// </summary>
        public ChemicalBond AddBond(Guid atom1Id, Guid atom2Id, BondType type)
        {
            var atom1 = Atoms.FirstOrDefault(a => a.Id == atom1Id);
            var atom2 = Atoms.FirstOrDefault(a => a.Id == atom2Id);

            if (atom1 == null || atom2 == null)
                throw new InvalidOperationException("Átomos não encontrados no grafo");

            var bond = new ChemicalBond(atom1Id, atom2Id, type);
            Bonds.Add(bond);
            return bond;
        }

        /// <summary>
        /// Remove uma ligação do grafo
        /// </summary>
        public void RemoveBond(ChemicalBond bond)
        {
            Bonds.Remove(bond);
        }

        /// <summary>
        /// Retorna todas as ligações de um átomo específico
        /// </summary>
        public List<ChemicalBond> GetBondsForAtom(Guid atomId)
        {
            return Bonds.Where(b => b.InvolveAtom(atomId)).ToList();
        }

        /// <summary>
        /// Retorna os vizinhos (átomos conectados) de um átomo
        /// </summary>
        public List<Atom> GetNeighbors(Guid atomId)
        {
            var neighborIds = Bonds
                .Where(b => b.InvolveAtom(atomId))
                .Select(b => b.GetOtherAtom(atomId))
                .ToList();

            return Atoms.Where(a => neighborIds.Contains(a.Id)).ToList();
        }

        /// <summary>
        /// Verifica se a molécula está balanceada (conservação de carga e valência)
        /// </summary>
        public bool IsValid()
        {
            foreach (var atom in Atoms)
            {
                var bondCount = GetBondsForAtom(atom.Id).Sum(b => (int)b.Type);
                if (bondCount > atom.MaxBonds)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Átomo {atom.Element} excede valência máxima!");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gera fórmula molecular empírica (ex: H2O, C6H12O6)
        /// </summary>
        public string GenerateFormula()
        {
            var elementCounts = Atoms
                .GroupBy(a => a.Element)
                .OrderBy(g => GetElementOrder(g.Key))
                .Select(g => new { Element = g.Key, Count = g.Count() });

            var formula = new StringBuilder();
            foreach (var ec in elementCounts)
            {
                formula.Append(ec.Element);
                if (ec.Count > 1)
                    formula.Append(ec.Count);
            }

            Formula = formula.ToString();
            return Formula;
        }

        /// <summary>
        /// Ordem padrão dos elementos na fórmula (Hill System)
        /// </summary>
        private int GetElementOrder(string element)
        {
            return element switch
            {
                "C" => 1,
                "H" => 2,
                _ => 3
            };
        }

        /// <summary>
        /// Clona o grafo molecular (deep copy)
        /// </summary>
        public MoleculeGraph Clone()
        {
            var clone = new MoleculeGraph
            {
                Formula = this.Formula,
                Name = this.Name
            };

            // Mapeia IDs antigos → novos
            var atomMap = new Dictionary<Guid, Guid>();

            foreach (var atom in Atoms)
            {
                var newAtom = clone.AddAtom(atom.Element);
                newAtom.Charge = atom.Charge;
                atomMap[atom.Id] = newAtom.Id;
            }

            foreach (var bond in Bonds)
            {
                clone.AddBond(atomMap[bond.Atom1Id], atomMap[bond.Atom2Id], bond.Type);
            }

            return clone;
        }

        public override string ToString()
        {
            return $"{Name} ({Formula}) - {Atoms.Count} átomos, {Bonds.Count} ligações";
        }
    }
}
