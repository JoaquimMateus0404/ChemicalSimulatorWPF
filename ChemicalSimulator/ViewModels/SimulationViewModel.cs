using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ChemicalSimulator.Models;
using ChemicalSimulator.Services;

namespace ChemicalSimulator.ViewModels
{
    /// <summary>
    /// ViewModel para simulação de reações químicas
    /// </summary>
    public class SimulationViewModel : INotifyPropertyChanged
    {
        private readonly ReactionPredictor _reactionPredictor;
        private readonly ChemistryEngine _chemistryEngine;

        private Reaction _currentReaction;
        private ReactionConditions _conditions;
        private bool _isSimulating;
        private double _simulationProgress;
        private string _statusMessage;
        private ObservableCollection<Molecule> _reactants;
        private ObservableCollection<Molecule> _products;

        public SimulationViewModel()
        {
            _reactionPredictor = new ReactionPredictor();
            _chemistryEngine = new ChemistryEngine();

            Conditions = new ReactionConditions();
            Reactants = new ObservableCollection<Molecule>();
            Products = new ObservableCollection<Molecule>();

            InitializeCommands();
        }

        #region Properties

        public Reaction CurrentReaction
        {
            get => _currentReaction;
            set
            {
                _currentReaction = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasReaction));
            }
        }

        public ReactionConditions Conditions
        {
            get => _conditions;
            set
            {
                _conditions = value;
                OnPropertyChanged();
            }
        }

        public bool IsSimulating
        {
            get => _isSimulating;
            set
            {
                _isSimulating = value;
                OnPropertyChanged();
            }
        }

        public double SimulationProgress
        {
            get => _simulationProgress;
            set
            {
                _simulationProgress = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Molecule> Reactants
        {
            get => _reactants;
            set
            {
                _reactants = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Molecule> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged();
            }
        }

        public bool HasReaction => CurrentReaction != null;

        #endregion

        #region Commands

        public ICommand StartSimulationCommand { get; private set; }
        public ICommand StopSimulationCommand { get; private set; }
        public ICommand AddReactantCommand { get; private set; }
        public ICommand RemoveReactantCommand { get; private set; }
        public ICommand ResetSimulationCommand { get; private set; }
        public ICommand AnalyzeReactionCommand { get; private set; }

        #endregion

        private void InitializeCommands()
        {
            StartSimulationCommand = new RelayCommand(async () => await StartSimulation(), CanStartSimulation);
            StopSimulationCommand = new RelayCommand(StopSimulation, () => IsSimulating);
            AddReactantCommand = new RelayCommand<Molecule>(AddReactant);
            RemoveReactantCommand = new RelayCommand<Molecule>(RemoveReactant);
            ResetSimulationCommand = new RelayCommand(ResetSimulation);
            AnalyzeReactionCommand = new RelayCommand(AnalyzeReaction, () => HasReaction);
        }

        #region Command Methods

        private bool CanStartSimulation()
        {
            return Reactants.Count > 0 && !IsSimulating;
        }

        private async Task StartSimulation()
        {
            IsSimulating = true;
            SimulationProgress = 0;
            StatusMessage = "Iniciando simulação...";

            try
            {
                // Etapa 1: Preparação (10%)
                await SimulateStep("Preparando moléculas...", 10);

                // Etapa 2: Análise termodinâmica (30%)
                await SimulateStep("Calculando termodinâmica...", 30);

                // Criar reação com primeiro reagente
                if (Reactants.Count > 0)
                {
                    CurrentReaction = _reactionPredictor.PredictReaction(Reactants[0], Conditions);
                }

                // Etapa 3: Cálculo de cinética (50%)
                await SimulateStep("Calculando cinética...", 50);
                if (CurrentReaction != null)
                {
                    CurrentReaction.CalculateThermodynamics();
                }

                // Etapa 4: Previsão de produtos (70%)
                await SimulateStep("Prevendo produtos...", 70);
                if (CurrentReaction != null)
                {
                    CurrentReaction.CalculateReactionRate();
                    UpdateProducts();
                }

                // Etapa 5: Finalização (100%)
                await SimulateStep("Finalizando simulação...", 100);

                StatusMessage = (CurrentReaction != null && CurrentReaction.IsSpontaneous)
                    ? "✓ Simulação concluída - Reação espontânea!"
                    : "✓ Simulação concluída - Reação não-espontânea";
            }
            catch (Exception ex)
            {
                StatusMessage = $"✗ Erro na simulação: {ex.Message}";
                MessageBox.Show($"Erro durante a simulação:\n{ex.Message}",
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsSimulating = false;
            }
        }

        private async Task SimulateStep(string message, double progress)
        {
            StatusMessage = message;

            // Animar o progresso
            while (SimulationProgress < progress)
            {
                SimulationProgress += 1;
                await Task.Delay(20);
            }

            SimulationProgress = progress;
        }

        private void StopSimulation()
        {
            IsSimulating = false;
            StatusMessage = "Simulação interrompida pelo usuário";
        }

        private void AddReactant(Molecule molecule)
        {
            if (molecule != null)
            {
                Reactants.Add(molecule);
                StatusMessage = $"Reagente adicionado: {molecule.Name}";
            }
        }

        private void RemoveReactant(Molecule molecule)
        {
            if (molecule != null)
            {
                Reactants.Remove(molecule);
                StatusMessage = $"Reagente removido: {molecule.Name}";
            }
        }

        private void ResetSimulation()
        {
            CurrentReaction = null;
            Reactants.Clear();
            Products.Clear();
            SimulationProgress = 0;
            StatusMessage = "Simulação resetada";
        }

        private void AnalyzeReaction()
        {
            if (CurrentReaction == null) return;

            var analysis = GenerateReactionAnalysis();

            MessageBox.Show(analysis, "Análise da Reação",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Helper Methods

        private void UpdateProducts()
        {
            Products.Clear();

            if (CurrentReaction?.Products != null)
            {
                foreach (var product in CurrentReaction.Products)
                {
                    Products.Add(product.Molecule);
                }
            }
        }

        private string GenerateReactionAnalysis()
        {
            if (CurrentReaction == null) return "Nenhuma reação simulada.";

            var analysis = new System.Text.StringBuilder();

            analysis.AppendLine("═══ ANÁLISE DA REAÇÃO ═══");
            analysis.AppendLine();

            analysis.AppendLine("TERMODINÂMICA:");
            analysis.AppendLine($"ΔH = {CurrentReaction.EnthalpyChange:F2} kJ/mol");
            analysis.AppendLine($"  → {(CurrentReaction.IsExothermic ? "EXOTÉRMICA (libera calor)" : "ENDOTÉRMICA (absorve calor)")}");
            analysis.AppendLine();

            analysis.AppendLine($"ΔG = {CurrentReaction.GibbsFreeEnergy:F2} kJ/mol");
            analysis.AppendLine($"  → {(CurrentReaction.IsSpontaneous ? "ESPONTÂNEA" : "NÃO-ESPONTÂNEA")}");
            analysis.AppendLine();

            analysis.AppendLine("CINÉTICA:");
            analysis.AppendLine($"Ea = {CurrentReaction.ActivationEnergy:F2} kJ/mol");
            analysis.AppendLine($"k = {CurrentReaction.RateConstant:E3} s⁻¹");
            analysis.AppendLine($"Ordem: {CurrentReaction.ReactionOrder}");
            analysis.AppendLine();

            analysis.AppendLine("CONDIÇÕES:");
            analysis.AppendLine($"T = {Conditions.Temperature:F2} K ({Conditions.Temperature - 273.15:F2} °C)");
            analysis.AppendLine($"P = {Conditions.Pressure:F2} kPa");
            analysis.AppendLine($"pH = {Conditions.pH:F2}");

            if (Conditions.Catalyst != "None" && !string.IsNullOrEmpty(Conditions.Catalyst))
                analysis.AppendLine($"Catalisador: {Conditions.Catalyst}");

            return analysis.ToString();
        }

        public void SetReactants(params Molecule[] molecules)
        {
            Reactants.Clear();
            foreach (var molecule in molecules)
            {
                Reactants.Add(molecule);
            }
        }

        public void UpdateConditions(double temperature, double pressure, double pH)
        {
            Conditions.Temperature = temperature;
            Conditions.Pressure = pressure;
            Conditions.pH = pH;
            OnPropertyChanged(nameof(Conditions));
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}