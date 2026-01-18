using System;
using System.Collections.Generic;
using System.Linq;

namespace ChemicalSimulator.Services.Chemistry.Rules
{
    /// <summary>
    /// Regra de Neutralização: Ácido + Base → Sal + H₂O
    /// </summary>
    public class NeutralizationRule : ChemicalRule
    {
        public NeutralizationRule(ElementClassifier elementClassifier) : base(elementClassifier)
        {
            Name = "Neutralização (Ácido-Base)";
            Type = ReactionType.Neutralization;
            Priority = 85;
            ActivationEnergy = 50;
            EnthalpyChange = -57; // Exotérmica
        }

        public override bool CanApply(List<MoleculeGraph> reactants, ReactionConditions? conditions = null)
        {
            if (reactants.Count < 2) return false;

            bool hasAcid = reactants.Any(r => IsAcid(r));
            bool hasBase = reactants.Any(r => IsBase(r));

            System.Diagnostics.Debug.WriteLine($"⚗️ NeutralizationRule.CanApply: Ácido={hasAcid}, Base={hasBase}");

            return hasAcid && hasBase;
        }

        public override List<MoleculeGraph> Apply(List<MoleculeGraph> reactants, ReactionConditions? conditions = null)
        {
            System.Diagnostics.Debug.WriteLine("⚗️ Aplicando regra de NEUTRALIZAÇÃO...");

            var products = new List<MoleculeGraph>();

            // Produto 1: H₂O (sempre)
            var h2o = new MoleculeGraph { Name = "Água", Formula = "H₂O" };
            var o = h2o.AddAtom("O");
            var h1 = h2o.AddAtom("H");
            var h2 = h2o.AddAtom("H");
            h2o.AddBond(o.Id, h1.Id, BondType.Single);
            h2o.AddBond(o.Id, h2.Id, BondType.Single);
            products.Add(h2o);

            // Produto 2: Sal (exemplo: NaCl)
            var salt = PredictSalt(reactants);
            if (salt != null)
            {
                products.Add(salt);
            }

            return products;
        }

        private bool IsAcid(MoleculeGraph molecule)
        {
            // Ácidos começam com H e têm outros elementos (HCl, H2SO4, etc.)
            var firstAtom = molecule.Atoms.FirstOrDefault();
            return firstAtom?.Element == "H" && molecule.Atoms.Any(a => a.Element != "H");
        }

        private bool IsBase(MoleculeGraph molecule)
        {
            // Bases contêm OH (NaOH, KOH, Ca(OH)2, etc.)
            bool hasO = molecule.Atoms.Any(a => a.Element == "O");
            bool hasH = molecule.Atoms.Any(a => a.Element == "H");
            bool hasMetal = molecule.Atoms.Any(a => IsMetal(a.Element));

            return hasO && hasH && hasMetal;
        }

        private bool IsMetal(string element)
        {
            var metals = new[] { "Na", "K", "Ca", "Mg", "Li", "Al", "Fe", "Cu", "Zn", "Ag" };
            return metals.Contains(element);
        }

        private MoleculeGraph? PredictSalt(List<MoleculeGraph> reactants)
        {
            // Lógica simplificada: HCl + NaOH → NaCl
            var acid = reactants.FirstOrDefault(r => IsAcid(r));
            var baseCompound = reactants.FirstOrDefault(r => IsBase(r));

            if (acid == null || baseCompound == null) return null;

            // Pega o metal da base e o ânion do ácido
            var metal = baseCompound.Atoms.FirstOrDefault(a => IsMetal(a.Element));
            var anion = acid.Atoms.FirstOrDefault(a => a.Element != "H");

            if (metal == null || anion == null) return null;

            // Cria sal (exemplo: NaCl)
            var salt = new MoleculeGraph { Name = "Sal", Formula = $"{metal.Element}{anion.Element}" };
            var m = salt.AddAtom(metal.Element);
            var a = salt.AddAtom(anion.Element);
            salt.AddBond(m.Id, a.Id, BondType.Ionic);

            return salt;
        }
    }
}
