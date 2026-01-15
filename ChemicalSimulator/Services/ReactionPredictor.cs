using ChemicalSimulator.Models;

namespace ChemicalSimulator.Services
{
    /// <summary>
    /// Preditor de reações químicas
    /// </summary>
    public class ReactionPredictor
    {
        private readonly ChemistryEngine _engine;

        public ReactionPredictor()
        {
            _engine = new ChemistryEngine();
        }

        public Reaction PredictReaction(Molecule reactant, ReactionConditions conditions)
        {
            var reaction = new Reaction
            {
                Conditions = conditions,
                Type = DetermineReactionType(reactant)
            };

            reaction.Reactants.Add(new ReactionComponent
            {
                Molecule = reactant,
                Coefficient = 1,
                Phase = "(l)"
            });

            // Prever produtos baseado no tipo de reação
            var products = PredictProducts(reactant, conditions);
            reaction.Products.AddRange(products);

            // Calcular propriedades termodinâmicas
            CalculateThermodynamics(reaction);

            // Calcular cinética
            CalculateKinetics(reaction);

            return reaction;
        }

        private ReactionType DetermineReactionType(Molecule reactant)
        {
            // Lógica simplificada para determinar tipo de reação
            if (ContainsOxygen(reactant))
                return ReactionType.Combustion;

            if (reactant.Atoms.Count > 10)
                return ReactionType.Decomposition;

            return ReactionType.Synthesis;
        }

        private List<ReactionComponent> PredictProducts(Molecule reactant, ReactionConditions conditions)
        {
            var products = new List<ReactionComponent>();

            // Exemplo: Combustão
            if (ContainsCarbon(reactant) && ContainsHydrogen(reactant))
            {
                // CxHy + O2 → CO2 + H2O
                products.Add(new ReactionComponent
                {
                    Molecule = new Molecule
                    {
                        Name = "Dióxido de Carbono",
                        Formula = "CO2",
                        MolarMass = 44.01,
                        EnthalpyOfFormation = -393.5
                    },
                    Coefficient = CountElement(reactant, "C"),
                    Phase = "(g)"
                });

                products.Add(new ReactionComponent
                {
                    Molecule = new Molecule
                    {
                        Name = "Água",
                        Formula = "H2O",
                        MolarMass = 18.015,
                        EnthalpyOfFormation = -285.8
                    },
                    Coefficient = CountElement(reactant, "H") / 2,
                    Phase = "(l)"
                });
            }

            return products;
        }

        private void CalculateThermodynamics(Reaction reaction)
        {
            // ΔH = Σ(ΔHf produtos) - Σ(ΔHf reagentes)
            double productsEnthalpy = reaction.Products
                .Sum(p => p.Coefficient * p.Molecule.EnthalpyOfFormation);

            double reactantsEnthalpy = reaction.Reactants
                .Sum(r => r.Coefficient * r.Molecule.EnthalpyOfFormation);

            reaction.EnthalpyChange = productsEnthalpy - reactantsEnthalpy;

            // Estimativa de entropia (simplificada)
            reaction.EntropyChange = EstimateEntropyChange(reaction);

            // ΔG = ΔH - TΔS
            reaction.GibbsFreeEnergy = reaction.EnthalpyChange -
                (reaction.Conditions.Temperature * reaction.EntropyChange / 1000.0);

            // Energia de ativação estimada
            reaction.ActivationEnergy = Math.Abs(reaction.EnthalpyChange) * 0.3 + 50;
        }

        private void CalculateKinetics(Reaction reaction)
        {
            // Lei de Arrhenius: k = A * e^(-Ea/RT)
            const double R = 8.314; // J/(mol·K)
            double A = 1e13; // Fator pré-exponencial

            reaction.RateConstant = A * Math.Exp(
                -reaction.ActivationEnergy * 1000 /
                (R * reaction.Conditions.Temperature));

            // Ordem da reação (simplificado)
            reaction.ReactionOrder = 2;

            // Tempo de meia-vida (para reação de primeira ordem)
            if (reaction.ReactionOrder == 1)
                reaction.HalfLife = Math.Log(2) / reaction.RateConstant;
        }

        private double EstimateEntropyChange(Reaction reaction)
        {
            // Estimativa baseada na mudança de fase e número de moléculas
            int productMolecules = reaction.Products.Sum(p => p.Coefficient);
            int reactantMolecules = reaction.Reactants.Sum(r => r.Coefficient);

            double deltaS = (productMolecules - reactantMolecules) * 50; // J/(mol·K)

            return deltaS;
        }

        private bool ContainsOxygen(Molecule molecule)
        {
            return molecule.Atoms.Any(a => a.Element.Symbol == "O");
        }

        private bool ContainsCarbon(Molecule molecule)
        {
            return molecule.Atoms.Any(a => a.Element.Symbol == "C");
        }

        private bool ContainsHydrogen(Molecule molecule)
        {
            return molecule.Atoms.Any(a => a.Element.Symbol == "H");
        }

        private int CountElement(Molecule molecule, string symbol)
        {
            return molecule.Atoms.Count(a => a.Element.Symbol == symbol);
        }

        public List<Reaction> SuggestPossibleReactions(Molecule reactant)
        {
            var suggestions = new List<Reaction>();

            // Adicionar várias reações possíveis
            var conditions1 = new ReactionConditions { Temperature = 298 };
            suggestions.Add(PredictReaction(reactant, conditions1));

            var conditions2 = new ReactionConditions { Temperature = 373, Catalyst = "Pt" };
            suggestions.Add(PredictReaction(reactant, conditions2));

            return suggestions;
        }
    }
}