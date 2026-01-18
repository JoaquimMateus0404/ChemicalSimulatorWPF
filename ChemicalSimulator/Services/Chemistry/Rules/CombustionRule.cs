using System;
using System.Collections.Generic;
using System.Linq;

namespace ChemicalSimulator.Services.Chemistry.Rules
{
    /// <summary>
    /// Regra de Combustão: CxHy + O2 → CO2 + H2O
    /// </summary>
    public class CombustionRule : ChemicalRule
    {
        public CombustionRule(ElementClassifier elementClassifier) : base(elementClassifier)
        {
            Name = "Combustão Completa";
            Type = ReactionType.Combustion;
            Priority = 90; // Alta prioridade
            ActivationEnergy = 150;
            EnthalpyChange = -890; // Exotérmica
        }

        public override bool CanApply(List<MoleculeGraph> reactants, ReactionConditions? conditions = null)
        {
            if (reactants.Count < 2) return false;

            // Precisa de um hidrocarboneto (C + H) + O₂
            bool hasHydrocarbon = reactants.Any(r => HasCarbon(r) && HasHydrogen(r));
            bool hasOxygen = reactants.Any(r => IsOxygen(r));

            System.Diagnostics.Debug.WriteLine($"🔥 CombustionRule.CanApply: Hidrocarboneto={hasHydrocarbon}, O2={hasOxygen}");

            return hasHydrocarbon && hasOxygen;
        }

        public override List<MoleculeGraph> Apply(List<MoleculeGraph> reactants, ReactionConditions? conditions = null)
        {
            System.Diagnostics.Debug.WriteLine("🔥 Aplicando regra de COMBUSTÃO...");

            var products = new List<MoleculeGraph>();

            // Produto 1: CO₂
            var co2 = new MoleculeGraph { Name = "Dióxido de Carbono", Formula = "CO₂" };
            var c = co2.AddAtom("C");
            var o1 = co2.AddAtom("O");
            var o2 = co2.AddAtom("O");
            co2.AddBond(c.Id, o1.Id, BondType.Double);
            co2.AddBond(c.Id, o2.Id, BondType.Double);
            products.Add(co2);

            // Produto 2: H₂O
            var h2o = new MoleculeGraph { Name = "Água", Formula = "H₂O" };
            var o = h2o.AddAtom("O");
            var h1 = h2o.AddAtom("H");
            var h2 = h2o.AddAtom("H");
            h2o.AddBond(o.Id, h1.Id, BondType.Single);
            h2o.AddBond(o.Id, h2.Id, BondType.Single);
            products.Add(h2o);

            System.Diagnostics.Debug.WriteLine($"✅ Produtos: {products.Count}");
            return products;
        }

        private bool HasCarbon(MoleculeGraph molecule)
        {
            return molecule.Atoms.Any(a => a.Element == "C");
        }

        private bool HasHydrogen(MoleculeGraph molecule)
        {
            return molecule.Atoms.Any(a => a.Element == "H");
        }

        private bool IsOxygen(MoleculeGraph molecule)
        {
            // O₂ tem exatamente 2 átomos de O
            return molecule.Atoms.Count == 2 && 
                   molecule.Atoms.All(a => a.Element == "O");
        }
    }
}
