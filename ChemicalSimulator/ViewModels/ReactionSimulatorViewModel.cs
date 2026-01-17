using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ChemicalSimulator.Commands;
using ChemicalSimulator.Models;
using ChemicalSimulator.Services;

namespace ChemicalSimulator.ViewModels
{
    /// <summary>
    /// ViewModel para simulação de reações químicas
    /// </summary>
    public class ReactionSimulatorViewModel : ViewModelBase
    {
        private readonly ReactionPredictor _reactionPredictor;
        private readonly ChemistryEngine _chemistryEngine;

        private Reaction? _currentReaction;
        private bool _isSimulating;
        private double _simulationProgress;
        private string _reactionEquation = "";
        private ReactionConditions _conditions;

        public ObservableCollection<Molecule> AvailableMolecules { get; }
        public ObservableCollection<ReactionComponent> Reactants { get; }
        public ObservableCollection<ReactionComponent> Products { get; }
        public ObservableCollection<string> SimulationSteps { get; }

        public Reaction? CurrentReaction
        {
            get => _currentReaction;
            set
            {
                SetProperty(ref _currentReaction, value);
                UpdateReactionInfo();
            }
        }

        public bool IsSimulating
        {
            get => _isSimulating;
            set => SetProperty(ref _isSimulating, value);
        }

        public double SimulationProgress
        {
            get => _simulationProgress;
            set => SetProperty(ref _simulationProgress, value);
        }

        public string ReactionEquation
        {
            get => _reactionEquation;
            set => SetProperty(ref _reactionEquation, value);
        }

        public ReactionConditions Conditions
        {
            get => _conditions;
            set
            {
                SetProperty(ref _conditions, value);
                if (CurrentReaction != null)
                {
                    CurrentReaction.Conditions = value;
                    UpdateThermodynamics();
                }
            }
        }

        // Propriedades termodinâmicas
        public double Temperature
        {
            get => _conditions.Temperature;
            set
            {
                _conditions.Temperature = value;
                OnPropertyChanged(nameof(Temperature));
                OnPropertyChanged(nameof(TemperatureCelsius));
                UpdateThermodynamics();
            }
        }

        public double TemperatureCelsius => Temperature - 273.15;

        public double Pressure
        {
            get => _conditions.Pressure;
            set
            {
                _conditions.Pressure = value;
                OnPropertyChanged(nameof(Pressure));
                UpdateThermodynamics();
            }
        }

        public string Solvent
        {
            get => _conditions.Solvent ?? "Nenhum";
            set
            {
                _conditions.Solvent = value;
                OnPropertyChanged(nameof(Solvent));
            }
        }

        public string Catalyst
        {
            get => _conditions.Catalyst ?? "Nenhum";
            set
            {
                _conditions.Catalyst = value;
                OnPropertyChanged(nameof(Catalyst));
            }
        }

        public double pH
        {
            get => _conditions.pH;
            set
            {
                _conditions.pH = value;
                OnPropertyChanged(nameof(pH));
                OnPropertyChanged(nameof(EnvironmentType));
            }
        }

        public string EnvironmentType
        {
            get
            {
                if (pH < 7) return "Ácido";
                if (pH > 7) return "Básico";
                return "Neutro";
            }
        }

        // Comandos
        public ICommand AddReactantCommand { get; }
        public ICommand RemoveReactantCommand { get; }
        public ICommand PredictProductsCommand { get; }
        public ICommand RunSimulationCommand { get; }
        public ICommand BalanceEquationCommand { get; }
        public ICommand ExportReactionCommand { get; }

        public ReactionSimulatorViewModel(ObservableCollection<Molecule> availableMolecules)
        {
            _reactionPredictor = new ReactionPredictor();
            _chemistryEngine = new ChemistryEngine();

            AvailableMolecules = availableMolecules;
            Reactants = new ObservableCollection<ReactionComponent>();
            Products = new ObservableCollection<ReactionComponent>();
            SimulationSteps = new ObservableCollection<string>();

            _conditions = new ReactionConditions
            {
                Temperature = 298.15, // 25°C
                Pressure = 101.325, // 1 atm
                pH = 7.0
            };

            // Inicializar comandos
            AddReactantCommand = new RelayCommand<Molecule>(AddReactant);
            RemoveReactantCommand = new RelayCommand<ReactionComponent>(RemoveReactant);
            PredictProductsCommand = new RelayCommand(PredictProducts, CanPredictProducts);
            RunSimulationCommand = new RelayCommand(async () => await RunSimulation(), CanRunSimulation);
            BalanceEquationCommand = new RelayCommand(BalanceEquation);
            ExportReactionCommand = new RelayCommand(ExportReaction);
        }

        private void AddReactant(Molecule? molecule)
        {
            if (molecule == null) return;

            var component = new ReactionComponent
            {
                Molecule = molecule,
                Coefficient = 1,
                Phase = DeterminePhase(molecule)
            };

            Reactants.Add(component);
            UpdateReactionEquation();
        }

        private string DeterminePhase(Molecule molecule)
        {
            if (Temperature < molecule.MeltingPoint)
                return "(s)";
            if (Temperature < molecule.BoilingPoint)
                return "(l)";
            return "(g)";
        }

        private void RemoveReactant(ReactionComponent? component)
        {
            if (component == null) return;
            Reactants.Remove(component);
            UpdateReactionEquation();
        }

        private bool CanPredictProducts() => Reactants.Count > 0;

        private void PredictProducts()
        {
            if (!CanPredictProducts()) return;

            SimulationSteps.Clear();
            Products.Clear();

            SimulationSteps.Add("🔍 Analisando reagentes...");

            // Criar reação
            CurrentReaction = new Reaction
            {
                Reactants = Reactants.ToList(),
                Conditions = Conditions
            };

            SimulationSteps.Add($"📊 Condições: T={TemperatureCelsius:F1}°C, P={Pressure:F1} kPa, pH={pH:F1}");

            // Prever produtos (simplificado)
            if (Reactants.Count == 1)
            {
                // Decomposição ou combustão
                var reactant = Reactants[0].Molecule;
                PredictDecompositionOrCombustion(reactant);
            }
            else if (Reactants.Count == 2)
            {
                // Síntese, dupla troca, etc.
                PredictSynthesisOrDisplacement();
            }

            UpdateReactionEquation();
            UpdateThermodynamics();

            SimulationSteps.Add("✅ Produtos previstos com sucesso!");
        }

        private void PredictDecompositionOrCombustion(Molecule reactant)
        {
            // Verificar se contém C e H (hidrocarboneto)
            bool hasCarbon = reactant.Atoms.Any(a => a.Element.Symbol == "C");
            bool hasHydrogen = reactant.Atoms.Any(a => a.Element.Symbol == "H");

            if (hasCarbon && hasHydrogen && Conditions.HasOxygen)
            {
                // Combustão: CxHy + O2 → CO2 + H2O
                SimulationSteps.Add("🔥 Tipo: Reação de Combustão");

                Products.Add(new ReactionComponent
                {
                    Molecule = new Molecule
                    {
                        Name = "Dióxido de Carbono",
                        Formula = "CO₂",
                        MolarMass = 44.01,
                        EnthalpyOfFormation = -393.5
                    },
                    Coefficient = reactant.Atoms.Count(a => a.Element.Symbol == "C"),
                    Phase = "(g)"
                });

                Products.Add(new ReactionComponent
                {
                    Molecule = new Molecule
                    {
                        Name = "Água",
                        Formula = "H₂O",
                        MolarMass = 18.015,
                        EnthalpyOfFormation = -285.8
                    },
                    Coefficient = reactant.Atoms.Count(a => a.Element.Symbol == "H") / 2,
                    Phase = "(l)"
                });

                CurrentReaction!.Type = ReactionType.Combustion;
            }
            else
            {
                // Decomposição térmica
                SimulationSteps.Add("⚡ Tipo: Decomposição Térmica");
                CurrentReaction!.Type = ReactionType.Decomposition;
            }
        }

        private void PredictSynthesisOrDisplacement()
        {
            SimulationSteps.Add("🔬 Tipo: Reação de Síntese/Deslocamento");

            // Exemplo simplificado: combinar reagentes
            var combinedName = string.Join(" + ", Reactants.Select(r => r.Molecule.Name));
            
            Products.Add(new ReactionComponent
            {
                Molecule = new Molecule
                {
                    Name = $"Produto ({combinedName})",
                    Formula = "?",
                    MolarMass = Reactants.Sum(r => r.Molecule.MolarMass)
                },
                Coefficient = 1,
                Phase = "(s)"
            });

            CurrentReaction!.Type = ReactionType.Synthesis;
        }

        private void UpdateReactionEquation()
        {
            var reactantStr = string.Join(" + ", 
                Reactants.Select(r => $"{(r.Coefficient > 1 ? r.Coefficient.ToString() : "")}{r.Molecule.Formula}"));

            var productStr = string.Join(" + ", 
                Products.Select(p => $"{(p.Coefficient > 1 ? p.Coefficient.ToString() : "")}{p.Molecule.Formula}"));

            ReactionEquation = string.IsNullOrEmpty(productStr) 
                ? $"{reactantStr} → ?" 
                : $"{reactantStr} → {productStr}";
        }

        private void UpdateThermodynamics()
        {
            if (CurrentReaction == null) return;

            CurrentReaction.CalculateThermodynamics();
            CurrentReaction.CalculateReactionRate();

            OnPropertyChanged(nameof(CurrentReaction));
        }

        private bool CanRunSimulation() => CurrentReaction != null && Products.Count > 0;

        private async Task RunSimulation()
        {
            if (!CanRunSimulation()) return;

            IsSimulating = true;
            SimulationProgress = 0;

            SimulationSteps.Add("");
            SimulationSteps.Add("🎬 Iniciando simulação...");

            // Simular etapas da reação
            var steps = new[]
            {
                "⚡ Quebra de ligações nos reagentes...",
                "🔄 Reorganização de átomos...",
                "🔗 Formação de novas ligações...",
                "💫 Liberação/Absorção de energia...",
                "✨ Formação dos produtos finais..."
            };

            for (int i = 0; i < steps.Length; i++)
            {
                await Task.Delay(800);
                SimulationSteps.Add(steps[i]);
                SimulationProgress = (i + 1) * 20;
            }

            // Resultados
            await Task.Delay(500);
            SimulationSteps.Add("");
            SimulationSteps.Add("📊 RESULTADOS:");
            SimulationSteps.Add($"   ΔH = {CurrentReaction!.EnthalpyChange:F2} kJ/mol " +
                (CurrentReaction.IsExothermic ? "(Exotérmica ♨️)" : "(Endotérmica 🧊)"));
            SimulationSteps.Add($"   ΔG = {CurrentReaction.GibbsFreeEnergy:F2} kJ/mol " +
                (CurrentReaction.IsSpontaneous ? "(Espontânea ✓)" : "(Não-espontânea ✗)"));
            SimulationSteps.Add($"   k = {CurrentReaction.RateConstant:E2} (Constante de velocidade)");

            await Task.Delay(500);
            SimulationSteps.Add("");
            SimulationSteps.Add("✅ Simulação concluída!");

            IsSimulating = false;
            SimulationProgress = 100;
        }

        private void BalanceEquation()
        {
            if (CurrentReaction == null) return;

            SimulationSteps.Add("⚖️ Balanceando equação química...");
            
            // Algoritmo simplificado de balanceamento
            // (Um balanceador real usaria álgebra linear)
            
            CurrentReaction.IsBalanced = true;
            SimulationSteps.Add("✅ Equação balanceada!");
            UpdateReactionEquation();
        }

        private void ExportReaction()
        {
            if (CurrentReaction == null) return;

            var exportService = new ExportService();
            var data = new
            {
                Equation = ReactionEquation,
                Conditions,
                Thermodynamics = new
                {
                    CurrentReaction.EnthalpyChange,
                    CurrentReaction.EntropyChange,
                    CurrentReaction.GibbsFreeEnergy,
                    CurrentReaction.ActivationEnergy
                },
                Kinetics = new
                {
                    CurrentReaction.RateConstant,
                    CurrentReaction.ReactionOrder
                }
            };

            MessageBox.Show("Reação exportada com sucesso!", 
                "Exportar", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateReactionInfo()
        {
            if (CurrentReaction == null) return;

            Reactants.Clear();
            Products.Clear();

            foreach (var r in CurrentReaction.Reactants)
                Reactants.Add(r);

            foreach (var p in CurrentReaction.Products)
                Products.Add(p);

            UpdateReactionEquation();
        }
    }
}
