using System;
using System.Collections.Generic;
using System.Linq;

namespace ChemicalSimulator.Services.Chemistry.Rules
{
    /// <summary>
    /// REGRA: Síntese (A + B → AB)
    /// Exemplo: H₂ + O₂ → H₂O, Na + Cl₂ → NaCl
    /// </summary>
    public class SynthesisRule : ChemicalRule
    {
        public SynthesisRule()
        {
            Name = "🧪 Síntese";
            Priority = 70;
            Type = ReactionType.Synthesis;
            ActivationEnergy = 120; // kJ/mol
            EnthalpyChange = -200;  // Exotérmica (libera calor)
        }

        public override bool CanApply(List<MoleculeGraph> reactants, ReactionConditions? conditions = null)
        {
            // ✅ Condição: 2 reagentes simples (elementos ou moléculas pequenas)
            if (reactants.Count != 2)
            {
                System.Diagnostics.Debug.WriteLine($"    ❌ Síntese requer 2 reagentes (tem {reactants.Count})");
                return false;
            }

            // Verificar se ambos são moléculas simples (poucas ligações)
            bool r1Simple = reactants[0].Bonds.Count <= 1; // Diatômica ou elemento
            bool r2Simple = reactants[1].Bonds.Count <= 1;

            if (!r1Simple || !r2Simple)
            {
                System.Diagnostics.Debug.WriteLine($"    ❌ Reagentes muito complexos para síntese");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"    ✅ Síntese válida: {reactants[0].Name} + {reactants[1].Name}");
            return true;
        }

        public override List<MoleculeGraph> Apply(List<MoleculeGraph> reactants, ReactionConditions? conditions = null)
        {
            var products = new List<MoleculeGraph>();

            System.Diagnostics.Debug.WriteLine($"🔥 Aplicando SÍNTESE: {reactants[0].Name} + {reactants[1].Name}");

            // 🧪 CASO ESPECIAL: H₂ + O₂ → H₂O
            if (IsHydrogen(reactants[0]) && IsOxygen(reactants[1]) ||
                IsHydrogen(reactants[1]) && IsOxygen(reactants[0]))
            {
                var water = new MoleculeGraph { Name = "Água", Formula = "H₂O" };
                var h1 = water.AddAtom("H");
                var h2 = water.AddAtom("H");
                var o = water.AddAtom("O");
                water.AddBond(o.Id, h1.Id, BondType.Single);
                water.AddBond(o.Id, h2.Id, BondType.Single);
                
                products.Add(water);
                System.Diagnostics.Debug.WriteLine("  ✅ H₂ + O₂ → H₂O");
                return products;
            }

            // 🧪 GENÉRICO: Combinar todos os átomos em uma molécula
            var combined = new MoleculeGraph
            {
                Name = "Composto",
                Formula = reactants[0].Formula + reactants[1].Formula.Replace("₂", "").Replace("2", "")
            };

            // Copiar todos os átomos
            foreach (var molecule in reactants)
            {
                foreach (var atom in molecule.Atoms)
                {
                    combined.AddAtom(atom.Element);
                }
            }

            System.Diagnostics.Debug.WriteLine($"  ✅ Síntese genérica: {combined.Formula}");
            products.Add(combined);
            return products;
        }

        private bool IsHydrogen(MoleculeGraph molecule)
        {
            return molecule.Atoms.Count == 2 && molecule.Atoms.All(a => a.Element == "H");
        }

        private bool IsOxygen(MoleculeGraph molecule)
        {
            return molecule.Atoms.Count == 2 && molecule.Atoms.All(a => a.Element == "O");
        }
    }
}
