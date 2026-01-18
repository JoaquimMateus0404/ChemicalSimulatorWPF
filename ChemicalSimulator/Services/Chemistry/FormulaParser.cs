using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace ChemicalSimulator.Services.Chemistry
{
    /// <summary>
    /// Parser de fórmulas químicas que gera MoleculeGraph
    /// Exemplo: H2O → grafo com 2H + 1O com ligações
    /// </summary>
    public class FormulaParser
    {
        /// <summary>
        /// Converte fórmula química em grafo molecular
        /// </summary>
        public MoleculeGraph ParseToGraph(string formula, string name = "")
        {
            System.Diagnostics.Debug.WriteLine($"📝 Parseando fórmula: {formula}");

            var graph = new MoleculeGraph
            {
                Formula = formula,
                Name = string.IsNullOrEmpty(name) ? formula : name
            };

            // Normalizar subscripts Unicode → ASCII
            formula = NormalizeFormula(formula);

            // Extrair elementos e quantidades
            var elementCounts = ParseElementCounts(formula);

            System.Diagnostics.Debug.WriteLine($"   Elementos encontrados: {string.Join(", ", elementCounts.Select(e => $"{e.Key}={e.Value}"))}");

            // Adicionar átomos ao grafo
            var atomsByElement = new Dictionary<string, List<Atom>>();

            foreach (var element in elementCounts)
            {
                atomsByElement[element.Key] = new List<Atom>();

                for (int i = 0; i < element.Value; i++)
                {
                    var atom = graph.AddAtom(element.Key);
                    atomsByElement[element.Key].Add(atom);
                    System.Diagnostics.Debug.WriteLine($"   ➕ Adicionado: {element.Key} (#{i + 1})");
                }
            }

            // Adicionar ligações (heurística simples)
            AddBondsHeuristic(graph, atomsByElement, formula);

            System.Diagnostics.Debug.WriteLine($"✅ Grafo criado: {graph.Atoms.Count} átomos, {graph.Bonds.Count} ligações");

            return graph;
        }

        /// <summary>
        /// Normaliza subscripts Unicode (₂) → ASCII (2)
        /// </summary>
        private string NormalizeFormula(string formula)
        {
            var subscripts = new Dictionary<char, char>
            {
                { '₀', '0' }, { '₁', '1' }, { '₂', '2' }, { '₃', '3' }, { '₄', '4' },
                { '₅', '5' }, { '₆', '6' }, { '₇', '7' }, { '₈', '8' }, { '₉', '9' }
            };

            foreach (var sub in subscripts)
            {
                formula = formula.Replace(sub.Key, sub.Value);
            }

            return formula;
        }

        /// <summary>
        /// Extrai elementos e suas quantidades da fórmula
        /// Exemplo: H2O → { H: 2, O: 1 }, CH4 → { C: 1, H: 4 }
        /// </summary>
        private Dictionary<string, int> ParseElementCounts(string formula)
        {
            var counts = new Dictionary<string, int>();

            // Regex: Elemento maiúsculo + minúscula opcional + dígitos opcionais
            // Exemplo: H2, Ca, Cl2, CH4
            var pattern = @"([A-Z][a-z]?)(\d*)";
            var matches = Regex.Matches(formula, pattern);

            foreach (Match match in matches)
            {
                string element = match.Groups[1].Value;
                string countStr = match.Groups[2].Value;

                if (string.IsNullOrEmpty(element)) continue;

                int count = string.IsNullOrEmpty(countStr) ? 1 : int.Parse(countStr);

                if (!counts.ContainsKey(element))
                    counts[element] = 0;

                counts[element] += count;
            }

            return counts;
        }

        /// <summary>
        /// Adiciona ligações baseadas em heurísticas químicas
        /// </summary>
        private void AddBondsHeuristic(MoleculeGraph graph, Dictionary<string, List<Atom>> atomsByElement, string formula)
        {
            // Heurística 1: Moléculas diatômicas (O2, H2, Cl2, etc.)
            if (graph.Atoms.Count == 2 && atomsByElement.Count == 1)
            {
                var atoms = graph.Atoms;
                graph.AddBond(atoms[0].Id, atoms[1].Id, BondType.Double);
                System.Diagnostics.Debug.WriteLine("   🔗 Molécula diatômica: ligação dupla");
                return;
            }

            // Heurística 2: Água H2O (O central)
            if (formula == "H2O")
            {
                var o = atomsByElement["O"][0];
                var h1 = atomsByElement["H"][0];
                var h2 = atomsByElement["H"][1];
                graph.AddBond(o.Id, h1.Id, BondType.Single);
                graph.AddBond(o.Id, h2.Id, BondType.Single);
                System.Diagnostics.Debug.WriteLine("   🔗 H2O: O-H-H");
                return;
            }

            // Heurística 3: CO2 (O=C=O)
            if (formula == "CO2")
            {
                var c = atomsByElement["C"][0];
                var o1 = atomsByElement["O"][0];
                var o2 = atomsByElement["O"][1];
                graph.AddBond(c.Id, o1.Id, BondType.Double);
                graph.AddBond(c.Id, o2.Id, BondType.Double);
                System.Diagnostics.Debug.WriteLine("   🔗 CO2: O=C=O");
                return;
            }

            // Heurística 4: Metano CH4 (C central)
            if (formula == "CH4")
            {
                var c = atomsByElement["C"][0];
                foreach (var h in atomsByElement["H"])
                {
                    graph.AddBond(c.Id, h.Id, BondType.Single);
                }
                System.Diagnostics.Debug.WriteLine("   🔗 CH4: C-H×4");
                return;
            }

            // Heurística 5: Sais iônicos (NaCl, etc.)
            if (atomsByElement.Count == 2 && graph.Atoms.Count == 2)
            {
                var atoms = graph.Atoms;
                var atom1 = atoms[0];
                var atom2 = atoms[1];

                // Se um é metal e outro não-metal → ligação iônica
                bool isMetal1 = IsMetal(atom1.Element);
                bool isMetal2 = IsMetal(atom2.Element);

                if (isMetal1 != isMetal2)
                {
                    graph.AddBond(atom1.Id, atom2.Id, BondType.Ionic);
                    System.Diagnostics.Debug.WriteLine("   🔗 Sal iônico: ligação iônica");
                    return;
                }
            }

            // Heurística 6: Default - conecta átomo central aos demais
            if (atomsByElement.Count > 1)
            {
                var centralElement = atomsByElement.OrderBy(e => e.Value.Count).Last().Key;
                var central = atomsByElement[centralElement][0];

                foreach (var element in atomsByElement.Keys)
                {
                    if (element == centralElement) continue;

                    foreach (var atom in atomsByElement[element])
                    {
                        graph.AddBond(central.Id, atom.Id, BondType.Single);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"   🔗 Estrutura genérica: {centralElement} central");
            }
        }

        private bool IsMetal(string element)
        {
            var metals = new[] { "Na", "K", "Ca", "Mg", "Li", "Al", "Fe", "Cu", "Zn", "Ag", "Au" };
            return metals.Contains(element);
        }
    }
}
