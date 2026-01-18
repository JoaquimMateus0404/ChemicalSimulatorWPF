using System;
using System.Collections.Generic;
using System.Linq;
using ChemicalSimulator.Services.Chemistry.Rules;

namespace ChemicalSimulator.Services.Chemistry
{
    /// <summary>
    /// Motor de Regras Químicas - aplica transformações químicas conhecidas
    /// </summary>
    public class ChemicalRuleEngine
    {
        private readonly List<ChemicalRule> _rules = new List<ChemicalRule>();
        private readonly ElementClassifier _elementClassifier;

        public ChemicalRuleEngine(ElementClassifier elementClassifier)
        {
            _elementClassifier = elementClassifier;
            InitializeRules();
        }

        /// <summary>
        /// Registra todas as regras químicas disponíveis
        /// </summary>
        private void InitializeRules()
        {
            // ✅ Regras ordenadas por prioridade (maior = mais específica)
            _rules.Add(new CombustionRule(_elementClassifier));              // 90 - Muito específica
            _rules.Add(new NeutralizationRule(_elementClassifier));          // 85 - Muito específica
            _rules.Add(new SynthesisRule(_elementClassifier));               // 70 - Média
            _rules.Add(new DecompositionRule(_elementClassifier));           // 60 - Média
            _rules.Add(new SingleDisplacementRule(_elementClassifier));      // 50 - Mais genérica

            _rules.Sort((a, b) => b.Priority.CompareTo(a.Priority)); // Ordem decrescente
            
            System.Diagnostics.Debug.WriteLine($"🧪 Motor de Regras inicializado com {_rules.Count} regras:");
            foreach (var rule in _rules)
            {
                System.Diagnostics.Debug.WriteLine($"   - {rule.Name} (prioridade={rule.Priority})");
            }
        }

        /// <summary>
        /// Encontra a regra aplicável aos reagentes
        /// </summary>
        public ChemicalRule? FindApplicableRule(List<MoleculeGraph> reactants, ReactionConditions? conditions = null)
        {
            System.Diagnostics.Debug.WriteLine($"🔍 Procurando regra para {reactants.Count} reagentes...");

            foreach (var rule in _rules)
            {
                System.Diagnostics.Debug.WriteLine($"  🧪 Testando: {rule.Name} (prioridade={rule.Priority})");
                
                if (rule.CanApply(reactants, conditions))
                {
                    System.Diagnostics.Debug.WriteLine($"  ✅ MATCH! Regra aplicável: {rule.Name}");
                    return rule;
                }
            }

            System.Diagnostics.Debug.WriteLine("  ❌ Nenhuma regra aplicável encontrada");
            return null;
        }

        /// <summary>
        /// Executa a reação e retorna produtos + metadados
        /// </summary>
        public ReactionResult ExecuteReaction(List<MoleculeGraph> reactants, ReactionConditions? conditions = null)
        {
            var result = new ReactionResult
            {
                Reactants = reactants
            };

            var rule = FindApplicableRule(reactants, conditions);

            if (rule == null)
            {
                result.Success = false;
                result.ReactionType = ReactionType.NoReaction;
                result.Message = "⚠️ Reagentes incompatíveis - reação não ocorre";
                System.Diagnostics.Debug.WriteLine(result.Message);
                return result;
            }

            try
            {
                result.Products = rule.Apply(reactants, conditions);
                result.ReactionType = rule.Type;
                result.ActivationEnergy = rule.ActivationEnergy;
                result.EnthalpyChange = rule.EnthalpyChange;
                result.Success = true;
                result.Message = $"✅ Reação: {rule.Name}";

                System.Diagnostics.Debug.WriteLine($"✅ Reação executada: {rule.Name}");
                System.Diagnostics.Debug.WriteLine($"   Produtos: {result.Products.Count}");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"❌ Erro ao aplicar regra: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(result.Message);
            }

            return result;
        }

        /// <summary>
        /// Retorna informações sobre todas as regras registradas
        /// </summary>
        public List<string> GetAvailableRules()
        {
            return _rules.Select(r => $"{r.Name} (Prioridade: {r.Priority})").ToList();
        }
    }

    /// <summary>
    /// Resultado da execução de uma reação
    /// </summary>
    public class ReactionResult
    {
        public bool Success { get; set; }
        public List<MoleculeGraph> Reactants { get; set; } = new List<MoleculeGraph>();
        public List<MoleculeGraph> Products { get; set; } = new List<MoleculeGraph>();
        public ReactionType ReactionType { get; set; }
        public double ActivationEnergy { get; set; }
        public double EnthalpyChange { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Retorna a equação balanceada
        /// </summary>
        public string GetEquation()
        {
            var reactantsStr = string.Join(" + ", Reactants.Select(r => r.Formula));
            var productsStr = string.Join(" + ", Products.Select(p => p.Formula));
            return $"{reactantsStr} → {productsStr}";
        }

        public override string ToString()
        {
            return Success ? GetEquation() : Message;
        }
    }
}
