using System;
using System.Collections.Generic;
using System.Linq;
using ChemicalSimulator.Models;

namespace ChemicalSimulator.Services.Chemistry
{
    /// <summary>
    /// Serviço profissional de predição de produtos químicos
    /// Integra o motor de regras com o sistema de compostos existente
    /// </summary>
    public class ReactionPredictionService
    {
        private readonly ChemicalRuleEngine _ruleEngine;
        private readonly FormulaParser _parser;

        public ReactionPredictionService()
        {
            _ruleEngine = new ChemicalRuleEngine();
            _parser = new FormulaParser();
        }

        /// <summary>
        /// Prediz produtos a partir de compostos do sistema
        /// </summary>
        public ReactionResult PredictProducts(List<Compound> reactantCompounds, ReactionConditions? conditions = null)
        {
            System.Diagnostics.Debug.WriteLine($"🧪 ReactionPredictionService.PredictProducts: {reactantCompounds.Count} reagentes");

            // Converter Compound → MoleculeGraph
            var reactantGraphs = new List<MoleculeGraph>();

            foreach (var compound in reactantCompounds)
            {
                try
                {
                    var graph = _parser.ParseToGraph(compound.Formula, compound.Name);
                    reactantGraphs.Add(graph);
                    System.Diagnostics.Debug.WriteLine($"  ✅ Parseado: {compound.Formula} → {graph.Atoms.Count} átomos");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"  ❌ Erro ao parsear {compound.Formula}: {ex.Message}");
                }
            }

            if (reactantGraphs.Count == 0)
            {
                return new ReactionResult
                {
                    Success = false,
                    Message = "❌ Não foi possível parsear os reagentes"
                };
            }

            // Executar reação
            var result = _ruleEngine.ExecuteReaction(reactantGraphs, conditions);

            System.Diagnostics.Debug.WriteLine($"📊 Resultado: {result.Message}");

            return result;
        }

        /// <summary>
        /// Converte MoleculeGraph de volta para Compound (para integração com UI)
        /// </summary>
        public Compound ConvertToCompound(MoleculeGraph graph)
        {
            return new Compound
            {
                Name = graph.Name,
                Formula = graph.GenerateFormula(),
                MolarMass = CalculateMolarMass(graph),
                MeltingPoint = null, // Não temos esses dados no grafo
                BoilingPoint = null
            };
        }

        /// <summary>
        /// Calcula massa molar baseada nos átomos
        /// </summary>
        private double CalculateMolarMass(MoleculeGraph graph)
        {
            var atomicMasses = new Dictionary<string, double>
            {
                { "H", 1.008 },
                { "C", 12.011 },
                { "N", 14.007 },
                { "O", 15.999 },
                { "F", 18.998 },
                { "Na", 22.990 },
                { "Mg", 24.305 },
                { "P", 30.974 },
                { "S", 32.06 },
                { "Cl", 35.45 },
                { "K", 39.098 },
                { "Ca", 40.078 },
                { "Br", 79.904 }
            };

            return graph.Atoms.Sum(atom =>
            {
                if (atomicMasses.TryGetValue(atom.Element, out double mass))
                    return mass;
                return 12.0; // Padrão se não encontrar
            });
        }

        /// <summary>
        /// Lista todas as regras químicas disponíveis
        /// </summary>
        public List<string> GetAvailableReactionTypes()
        {
            return _ruleEngine.GetAvailableRules();
        }
    }
}
