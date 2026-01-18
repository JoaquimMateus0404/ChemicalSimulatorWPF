using System;
using System.Collections.Generic;
using System.Linq;

namespace ChemicalSimulator.Services.Chemistry.Rules
{
    /// <summary>
    /// REGRA: Decomposição (AB → A + B)
    /// Exemplo: H₂O → H₂ + O₂, CaCO₃ → CaO + CO₂
    /// </summary>
    public class DecompositionRule : ChemicalRule
    {
        public DecompositionRule()
        {
            Name = "💥 Decomposição";
            Priority = 60;
            Type = ReactionType.Decomposition;
            ActivationEnergy = 200; // kJ/mol (requer energia)
            EnthalpyChange = 180;   // Endotérmica (absorve calor)
        }

        public override bool CanApply(List<MoleculeGraph> reactants, ReactionConditions? conditions = null)
        {
            // ✅ Condição: APENAS 1 reagente complexo (mais de 1 elemento diferente)
            if (reactants.Count != 1)
            {
                System.Diagnostics.Debug.WriteLine($"    ❌ Decomposição requer 1 reagente (tem {reactants.Count})");
                return false;
            }

            var molecule = reactants[0];
            var uniqueElements = molecule.Atoms.Select(a => a.Element).Distinct().Count();

            if (uniqueElements < 2)
            {
                System.Diagnostics.Debug.WriteLine($"    ❌ Molécula muito simples ({uniqueElements} elementos)");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"    ✅ Decomposição válida: {molecule.Name} ({uniqueElements} elementos)");
            return true;
        }

        public override List<MoleculeGraph> Apply(List<MoleculeGraph> reactants, ReactionConditions? conditions = null)
        {
            var products = new List<MoleculeGraph>();
            var molecule = reactants[0];

            System.Diagnostics.Debug.WriteLine($"🔥 Aplicando DECOMPOSIÇÃO: {molecule.Name}");

            // 🧪 CASO ESPECIAL: H₂O → H₂ + O₂
            if (IsWater(molecule))
            {
                products.Add(CreateMolecule("H₂", "Hidrogênio", new[] { "H", "H" }));
                products.Add(CreateMolecule("O₂", "Oxigênio", new[] { "O", "O" }));
                System.Diagnostics.Debug.WriteLine("  ✅ H₂O → H₂ + O₂");
                return products;
            }

            // 🧪 CASO ESPECIAL: H₂O₂ → H₂O + O₂
            if (molecule.Formula == "H₂O₂" || molecule.Formula == "H2O2")
            {
                products.Add(CreateMolecule("H₂O", "Água", new[] { "H", "H", "O" }));
                products.Add(CreateMolecule("O₂", "Oxigênio", new[] { "O", "O" }));
                System.Diagnostics.Debug.WriteLine("  ✅ H₂O₂ → H₂O + O₂");
                return products;
            }

            // 🧪 GENÉRICO: Quebrar em elementos constituintes
            var elementGroups = molecule.Atoms.GroupBy(a => a.Element);
            
            foreach (var group in elementGroups)
            {
                var element = group.Key;
                var count = group.Count();

                if (count == 1)
                {
                    // Átomo individual (raro em decomposição)
                    products.Add(CreateMolecule(element, element, new[] { element }));
                }
                else if (count == 2)
                {
                    // Molécula diatômica (H₂, O₂, Cl₂, etc.)
                    products.Add(CreateMolecule($"{element}₂", $"{element}₂", new[] { element, element }));
                }
                else
                {
                    // Múltiplos átomos → formar moléculas diatômicas
                    int pairs = count / 2;
                    for (int i = 0; i < pairs; i++)
                    {
                        products.Add(CreateMolecule($"{element}₂", $"{element}₂", new[] { element, element }));
                    }

                    // Se sobrou um átomo ímpar
                    if (count % 2 != 0)
                    {
                        products.Add(CreateMolecule(element, element, new[] { element }));
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"  ✅ Decomposição genérica: {products.Count} produtos");
            return products;
        }

        /// <summary>
        /// Detecta se a molécula é água
        /// </summary>
        private bool IsWater(MoleculeGraph molecule)
        {
            var formula = molecule.Formula.Replace("₂", "2");
            return formula == "H2O" || formula == "H₂O";
        }

        /// <summary>
        /// Cria uma molécula simples a partir de símbolos de elementos
        /// </summary>
        private MoleculeGraph CreateMolecule(string formula, string name, string[] elements)
        {
            var graph = new MoleculeGraph
            {
                Name = name,
                Formula = formula
            };

            var atomIds = new List<Guid>();

            // Adicionar átomos
            for (int i = 0; i < elements.Length; i++)
            {
                var atom = graph.AddAtom(elements[i]);
                atomIds.Add(atom.Id);
            }

            // Adicionar ligações (todos conectados linearmente para simplificar)
            for (int i = 0; i < atomIds.Count - 1; i++)
            {
                graph.AddBond(atomIds[i], atomIds[i + 1], BondType.Single);
            }

            return graph;
        }

        /// <summary>
        /// Retorna a valência típica de um elemento
        /// </summary>
        private int GetValence(string element)
        {
            return element switch
            {
                "H" => 1,
                "O" => 2,
                "N" => 3,
                "C" => 4,
                "Cl" => 1,
                "S" => 2,
                _ => 2 // Default
            };
        }
    }
}
