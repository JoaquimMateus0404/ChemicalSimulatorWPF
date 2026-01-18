using System;
using System.Collections.Generic;
using System.Linq;

namespace ChemicalSimulator.Services.Chemistry.Rules
{
    /// <summary>
    /// REGRA: Simples Troca (A + BC → AC + B)
    /// Exemplo: Zn + HCl → ZnCl₂ + H₂
    /// </summary>
    public class SingleDisplacementRule : ChemicalRule
    {
        public SingleDisplacementRule()
        {
            Name = "🔄 Simples Troca";
            Priority = 50;
            Type = ReactionType.SingleDisplacement;
            ActivationEnergy = 80; // kJ/mol
            EnthalpyChange = -50;  // Levemente exotérmica
        }

        public override bool CanApply(List<MoleculeGraph> reactants, ReactionConditions? conditions = null)
        {
            // ✅ Condição: 1 elemento simples + 1 composto
            if (reactants.Count != 2)
            {
                System.Diagnostics.Debug.WriteLine($"    ❌ Simples troca requer 2 reagentes (tem {reactants.Count})");
                return false;
            }

            bool hasElement = reactants[0].Atoms.Count <= 2 || reactants[1].Atoms.Count <= 2;
            bool hasCompound = reactants[0].Atoms.Count > 2 || reactants[1].Atoms.Count > 2;

            if (!hasElement || !hasCompound)
            {
                System.Diagnostics.Debug.WriteLine($"    ❌ Não tem elemento + composto");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"    ✅ Simples troca válida");
            return true;
        }

        public override List<MoleculeGraph> Apply(List<MoleculeGraph> reactants, ReactionConditions? conditions = null)
        {
            var products = new List<MoleculeGraph>();

            System.Diagnostics.Debug.WriteLine($"🔥 Aplicando SIMPLES TROCA");

            // Implementação simplificada - retorna reagentes inalterados
            // TODO: Implementar lógica de troca real baseada em reatividade
            
            System.Diagnostics.Debug.WriteLine("  ⚠️ Simples troca não implementada completamente");
            return products;
        }
    }
}
