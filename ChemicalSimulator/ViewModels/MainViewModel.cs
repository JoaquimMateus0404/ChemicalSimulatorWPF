using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ChemicalSimulator.Models;
using ChemicalSimulator.Services;

namespace ChemicalSimulator.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ChemistryEngine _chemistryEngine;
        private readonly ReactionPredictor _reactionPredictor;
        private readonly ExportService _exportService;

        #region Properties

        private Molecule _currentMolecule;
        public Molecule CurrentMolecule
        {
            get => _currentMolecule;
            set
            {
                _currentMolecule = value;
                OnPropertyChanged();
            }
        }

        private Reaction _reaction;
        public Reaction Reaction
        {
            get => _reaction;
            set
            {
                _reaction = value;
                OnPropertyChanged();
            }
        }

        private ReactionConditions _conditions;
        public ReactionConditions Conditions
        {
            get => _conditions;
            set
            {
                _conditions = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        private bool _isProcessing;
        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                _isProcessing = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Element> CommonElements { get; set; }
        public ObservableCollection<Molecule> CommonMolecules { get; set; }

        #endregion

        #region Commands

        public ICommand AddElementCommand { get; }
        public ICommand AddBondCommand { get; }
        public ICommand SimulateReactionCommand { get; }
        public ICommand ExportResultsCommand { get; }
        public ICommand ClearWorkspaceCommand { get; }
        public ICommand LoadMoleculeCommand { get; }

        #endregion

        public MainViewModel()
        {
            _chemistryEngine = new ChemistryEngine();
            _reactionPredictor = new ReactionPredictor();
            _exportService = new ExportService();

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
                CreateElement(1, "H", "Hidrogênio", 1.008),
                CreateElement(6, "C", "Carbono", 12.011),
                CreateElement(7, "N", "Nitrogênio", 14.007),
                CreateElement(8, "O", "Oxigênio", 15.999),
                CreateElement(9, "F", "Flúor", 18.998),
                CreateElement(11, "Na", "Sódio", 22.990),
                CreateElement(12, "Mg", "Magnésio", 24.305),
                CreateElement(15, "P", "Fósforo", 30.974),
                CreateElement(16, "S", "Enxofre", 32.065),
                CreateElement(17, "Cl", "Cloro", 35.453),
                CreateElement(19, "K", "Potássio", 39.098),
                CreateElement(20, "Ca", "Cálcio", 40.078),
                CreateElement(26, "Fe", "Ferro", 55.845),
                CreateElement(29, "Cu", "Cobre", 63.546),
                CreateElement(30, "Zn", "Zinco", 65.38),
                CreateElement(35, "Br", "Bromo", 79.904)
            };

            CommonMolecules = new ObservableCollection<Molecule>
            {
                CreateCommonMolecule("Água", "H2O", 18.015),
                CreateCommonMolecule("Metano", "CH4", 16.043),
                CreateCommonMolecule("Etanol", "C2H5OH", 46.069),
                CreateCommonMolecule("Ácido Acético", "CH3COOH", 60.052),
                CreateCommonMolecule("Glicose", "C6H12O6", 180.156),
                CreateCommonMolecule("Amônia", "NH3", 17.031),
                CreateCommonMolecule("Dióxido de Carbono", "CO2", 44.009),
                CreateCommonMolecule("Ácido Sulfúrico", "H2SO4", 98.079),
                CreateCommonMolecule("Cloreto de Sódio", "NaCl", 58.443),
                CreateCommonMolecule("Benzeno", "C6H6", 78.114)
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
            CurrentMolecule.CalculateMolarMass();
            CurrentMolecule.Formula = CurrentMolecule.GetMolecularFormula();

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

        #region Helper Methods

        private Element CreateElement(int atomicNumber, string symbol, string name, double mass)
        {
            return new Element(atomicNumber, symbol, name)
            {
                AtomicMass = mass,
                Category = DetermineCategory(atomicNumber)
            };
        }

        private ElementCategory DetermineCategory(int atomicNumber)
        {
            return atomicNumber switch
            {
                1 or 6 or 7 or 8 or 15 or 16 => ElementCategory.NonMetal,
                9 or 17 or 35 => ElementCategory.Halogen,
                11 or 19 => ElementCategory.AlkaliMetal,
                12 or 20 => ElementCategory.AlkalineEarthMetal,
                26 or 29 or 30 => ElementCategory.TransitionMetal,
                _ => ElementCategory.NonMetal
            };
        }

        private Molecule CreateCommonMolecule(string name, string formula, double molarMass)
        {
            return new Molecule
            {
                Name = name,
                Formula = formula,
                MolarMass = molarMass
            };
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

    // RelayCommand Helper
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object parameter) => _execute();
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object parameter) => _execute((T)parameter);
    }
}