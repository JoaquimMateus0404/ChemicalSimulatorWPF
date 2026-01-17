using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ChemicalSimulator.Commands;
using ChemicalSimulator.Helpers;
using ChemicalSimulator.Infrastructure;
using ChemicalSimulator.Models;
using ChemicalSimulator.Services;

namespace ChemicalSimulator.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ChemistryEngine _chemistryEngine;
        private readonly ReactionPredictor _reactionPredictor;
        private readonly ExportService _exportService;

        #region Properties

        private Molecule _currentMolecule;
        public Molecule CurrentMolecule
        {
            get => _currentMolecule;
            set => SetProperty(ref _currentMolecule, value);
        }

        private Reaction _reaction;
        public Reaction Reaction
        {
            get => _reaction;
            set => SetProperty(ref _reaction, value);
        }

        private ReactionConditions _conditions;
        public ReactionConditions Conditions
        {
            get => _conditions;
            set => SetProperty(ref _conditions, value);
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private bool _isProcessing;
        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        public ObservableCollection<Element> CommonElements { get; set; }
        public ObservableCollection<Molecule> CommonMolecules { get; set; }

        #endregion

        #region Commands

        public ICommand AddElementCommand { get; private set; }
        public ICommand AddBondCommand { get; private set; }
        public ICommand SimulateReactionCommand { get; private set; }
        public ICommand ExportResultsCommand { get; private set; }
        public ICommand ClearWorkspaceCommand { get; private set; }
        public ICommand LoadMoleculeCommand { get; private set; }

        #endregion

        public MainViewModel()
        {
            // Obter serviços do ServiceLocator
            _chemistryEngine = ServiceLocator.Instance.GetService<ChemistryEngine>();
            _reactionPredictor = ServiceLocator.Instance.GetService<ReactionPredictor>();
            _exportService = ServiceLocator.Instance.GetService<ExportService>();

            InitializeCollections();
            InitializeCommands();

            CurrentMolecule = new Molecule { Name = "Nova Molécula" };
            Conditions = new ReactionConditions();
            StatusMessage = "Pronto para começar";
        }

        private void InitializeCollections()
        {
            CommonElements = new ObservableCollection<Element>
            {
                ElementFactory.CreateElement(1, "H", "Hidrogênio", 1.008, 2.20),
                ElementFactory.CreateElement(6, "C", "Carbono", 12.011, 2.55),
                ElementFactory.CreateElement(7, "N", "Nitrogênio", 14.007, 3.04),
                ElementFactory.CreateElement(8, "O", "Oxigênio", 15.999, 3.44),
                ElementFactory.CreateElement(9, "F", "Flúor", 18.998, 3.98),
                ElementFactory.CreateElement(11, "Na", "Sódio", 22.990, 0.93),
                ElementFactory.CreateElement(12, "Mg", "Magnésio", 24.305, 1.31),
                ElementFactory.CreateElement(15, "P", "Fósforo", 30.974, 2.19),
                ElementFactory.CreateElement(16, "S", "Enxofre", 32.065, 2.58),
                ElementFactory.CreateElement(17, "Cl", "Cloro", 35.453, 3.16),
                ElementFactory.CreateElement(19, "K", "Potássio", 39.098, 0.82),
                ElementFactory.CreateElement(20, "Ca", "Cálcio", 40.078, 1.00),
                ElementFactory.CreateElement(26, "Fe", "Ferro", 55.845, 1.83),
                ElementFactory.CreateElement(29, "Cu", "Cobre", 63.546, 1.90),
                ElementFactory.CreateElement(30, "Zn", "Zinco", 65.38, 1.65),
                ElementFactory.CreateElement(35, "Br", "Bromo", 79.904, 2.96)
            };

            CommonMolecules = new ObservableCollection<Molecule>
            {
                ElementFactory.CreateCommonMolecule("Água", "H2O", 18.015, -285.8),
                ElementFactory.CreateCommonMolecule("Metano", "CH4", 16.043, -74.6),
                ElementFactory.CreateCommonMolecule("Etanol", "C2H5OH", 46.069, -277.6),
                ElementFactory.CreateCommonMolecule("Ácido Acético", "CH3COOH", 60.052, -484.5),
                ElementFactory.CreateCommonMolecule("Glicose", "C6H12O6", 180.156, -1273.3),
                ElementFactory.CreateCommonMolecule("Amônia", "NH3", 17.031, -45.9),
                ElementFactory.CreateCommonMolecule("Dióxido de Carbono", "CO2", 44.009, -393.5),
                ElementFactory.CreateCommonMolecule("Ácido Sulfúrico", "H2SO4", 98.079, -814.0),
                ElementFactory.CreateCommonMolecule("Cloreto de Sódio", "NaCl", 58.443, -411.2),
                ElementFactory.CreateCommonMolecule("Benzeno", "C6H6", 78.114, 49.0)
            };
        }

        private void InitializeCommands()
        {
            AddElementCommand = new RelayCommand<Element>(AddElement);
            AddBondCommand = new RelayCommand<BondType>(AddBond);
            SimulateReactionCommand = new RelayCommand(SimulateReaction, CanSimulate);
            ExportResultsCommand = new RelayCommand(ExportResults, CanExport);
            ClearWorkspaceCommand = new RelayCommand(ClearWorkspace);
            LoadMoleculeCommand = new RelayCommand<Molecule>(LoadMolecule);
        }

        #region Command Methods

        private void AddElement(Element element)
        {
            if (element == null) return;

            var position = new System.Windows.Media.Media3D.Point3D(
                CurrentMolecule.Atoms.Count * 2.0, 0, 0);

            CurrentMolecule.AddAtom(element, position);
            CurrentMolecule.MolarMass = ChemistryCalculator.CalculateMolarMass(CurrentMolecule);
            CurrentMolecule.Formula = ChemistryCalculator.GenerateMolecularFormula(CurrentMolecule);

            StatusMessage = $"Elemento {element.Symbol} adicionado";
            OnPropertyChanged(nameof(CurrentMolecule));
        }

        private void AddBond(BondType bondType)
        {
            // Lógica para adicionar ligação entre átomos selecionados
            StatusMessage = $"Modo de ligação: {bondType}";
        }

        private bool CanSimulate()
        {
            return CurrentMolecule?.Atoms.Count > 0 && !IsProcessing;
        }

        private async void SimulateReaction()
        {
            IsProcessing = true;
            StatusMessage = "Simulando reação...";

            try
            {
                // Simular delay para processamento
                await System.Threading.Tasks.Task.Delay(1000);

                Reaction = _reactionPredictor.PredictReaction(CurrentMolecule, Conditions);
                Reaction.CalculateThermodynamics();
                Reaction.CalculateReactionRate();

                StatusMessage = Reaction.IsSpontaneous
                    ? "Simulação concluída - Reação espontânea!"
                    : "Simulação concluída - Reação não-espontânea";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro na simulação: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private bool CanExport()
        {
            return Reaction != null && !IsProcessing;
        }

        private void ExportResults()
        {
            try
            {
                _exportService.ExportToPdf(Reaction, CurrentMolecule, Conditions);
                StatusMessage = "Resultados exportados com sucesso!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao exportar: {ex.Message}";
            }
        }

        private void ClearWorkspace()
        {
            CurrentMolecule = new Molecule { Name = "Nova Molécula" };
            Reaction = null;
            StatusMessage = "Área de trabalho limpa";
        }

        private void LoadMolecule(Molecule molecule)
        {
            if (molecule == null) return;

            CurrentMolecule = molecule;
            StatusMessage = $"Molécula carregada: {molecule.Name}";
        }

        #endregion
    }
}