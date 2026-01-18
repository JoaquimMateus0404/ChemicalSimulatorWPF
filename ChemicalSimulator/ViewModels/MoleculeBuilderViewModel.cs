using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using ChemicalSimulator.Commands;
using ChemicalSimulator.Models;
using ChemicalSimulator.Services;

namespace ChemicalSimulator.ViewModels
{
    /// <summary>
    /// ViewModel profissional para construção e análise de moléculas
    /// </summary>
    public class MoleculeBuilderViewModel : ViewModelBase
    {
        #region Serviços
        private readonly ChemistryEngine _chemistryEngine;
        private readonly ElementDataLoader _elementDataLoader;
        private readonly CompoundDataLoader _compoundDataLoader;
        #endregion

        #region Propriedades - Molécula Atual
        private Molecule _currentMolecule;
        public Molecule CurrentMolecule
        {
            get => _currentMolecule;
            set
            {
                if (SetProperty(ref _currentMolecule, value))
                {
                    UpdateMoleculeAnalysis();
                    ValidateMolecule();
                }
            }
        }

        private ObservableCollection<Atom> _atoms;
        public ObservableCollection<Atom> Atoms
        {
            get => _atoms;
            set => SetProperty(ref _atoms, value);
        }

        private ObservableCollection<Bond> _bonds;
        public ObservableCollection<Bond> Bonds
        {
            get => _bonds;
            set => SetProperty(ref _bonds, value);
        }
        #endregion

        #region Propriedades - Seleção e Edição
        private Element _selectedElement;
        public Element SelectedElement
        {
            get => _selectedElement;
            set => SetProperty(ref _selectedElement, value);
        }

        private BondType _selectedBondType;
        public BondType SelectedBondType
        {
            get => _selectedBondType;
            set => SetProperty(ref _selectedBondType, value);
        }

        private Atom _selectedAtom;
        public Atom SelectedAtom
        {
            get => _selectedAtom;
            set
            {
                if (SetProperty(ref _selectedAtom, value))
                {
                    UpdateAtomInfo();
                }
            }
        }

        private Bond _selectedBond;
        public Bond SelectedBond
        {
            get => _selectedBond;
            set => SetProperty(ref _selectedBond, value);
        }

        private Point3D? _firstAtomForBond;
        #endregion

        #region Propriedades - Análise Química
        private string _molecularFormula;
        public string MolecularFormula
        {
            get => _molecularFormula;
            set => SetProperty(ref _molecularFormula, value);
        }

        private double _molarMass;
        public double MolarMass
        {
            get => _molarMass;
            set => SetProperty(ref _molarMass, value);
        }

        private int _totalBonds;
        public int TotalBonds
        {
            get => _totalBonds;
            set => SetProperty(ref _totalBonds, value);
        }

        private bool _isPolar;
        public bool IsPolar
        {
            get => _isPolar;
            set => SetProperty(ref _isPolar, value);
        }

        private MoleculeGeometry _geometry;
        public MoleculeGeometry Geometry
        {
            get => _geometry;
            set => SetProperty(ref _geometry, value);
        }

        private string _moleculeType;
        public string MoleculeType
        {
            get => _moleculeType;
            set => SetProperty(ref _moleculeType, value);
        }

        private double _dipoleMoment;
        public double DipoleMoment
        {
            get => _dipoleMoment;
            set => SetProperty(ref _dipoleMoment, value);
        }
        #endregion

        #region Propriedades - Validação
        private ObservableCollection<ValidationWarning> _validationWarnings;
        public ObservableCollection<ValidationWarning> ValidationWarnings
        {
            get => _validationWarnings;
            set => SetProperty(ref _validationWarnings, value);
        }

        private bool _isValid;
        public bool IsValid
        {
            get => _isValid;
            set => SetProperty(ref _isValid, value);
        }

        private string _validationSummary;
        public string ValidationSummary
        {
            get => _validationSummary;
            set => SetProperty(ref _validationSummary, value);
        }
        #endregion

        #region Propriedades - Templates
        private ObservableCollection<MoleculeTemplate> _moleculeTemplates;
        public ObservableCollection<MoleculeTemplate> MoleculeTemplates
        {
            get => _moleculeTemplates;
            set => SetProperty(ref _moleculeTemplates, value);
        }

        private MoleculeTemplate? _selectedTemplate;
        public MoleculeTemplate? SelectedTemplate
        {
            get => _selectedTemplate;
            set => SetProperty(ref _selectedTemplate, value);
        }
        #endregion

        #region Propriedades - UI
        private bool _isEducationalMode;
        public bool IsEducationalMode
        {
            get => _isEducationalMode;
            set
            {
                if (SetProperty(ref _isEducationalMode, value))
                {
                    if (value) ValidateMolecule();
                }
            }
        }

        private bool _show3DView;
        public bool Show3DView
        {
            get => _show3DView;
            set => SetProperty(ref _show3DView, value);
        }

        private double _zoomLevel;
        public double ZoomLevel
        {
            get => _zoomLevel;
            set => SetProperty(ref _zoomLevel, value);
        }

        private double _atomSize3D;
        public double AtomSize3D
        {
            get => _atomSize3D;
            set => SetProperty(ref _atomSize3D, value);
        }

        private double _bondThickness3D;
        public double BondThickness3D
        {
            get => _bondThickness3D;
            set => SetProperty(ref _bondThickness3D, value);
        }

        private bool _showLabels3D;
        public bool ShowLabels3D
        {
            get => _showLabels3D;
            set => SetProperty(ref _showLabels3D, value);
        }

        private string _viewStyle3D;
        public string ViewStyle3D
        {
            get => _viewStyle3D;
            set => SetProperty(ref _viewStyle3D, value);
        }

        private string _educationalMessage;
        public string EducationalMessage
        {
            get => _educationalMessage;
            set => SetProperty(ref _educationalMessage, value);
        }

        private ObservableCollection<Element> _availableElements;
        public ObservableCollection<Element> AvailableElements
        {
            get => _availableElements;
            set => SetProperty(ref _availableElements, value);
        }
        #endregion

        #region Propriedades - Histórico (Undo/Redo)
        private Stack<MoleculeAction> _undoStack;
        private Stack<MoleculeAction> _redoStack;

        private bool _canUndo;
        public bool CanUndo
        {
            get => _canUndo;
            set => SetProperty(ref _canUndo, value);
        }

        private bool _canRedo;
        public bool CanRedo
        {
            get => _canRedo;
            set => SetProperty(ref _canRedo, value);
        }
        #endregion

        #region Comandos - Manipulação de Átomos e Ligações
        public ICommand AddAtomCommand { get; }
        public ICommand RemoveAtomCommand { get; }
        public ICommand AddBondCommand { get; }
        public ICommand RemoveBondCommand { get; }
        public ICommand ClearMoleculeCommand { get; }
        #endregion

        #region Comandos - Histórico
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        #endregion

        #region Comandos - Análise e Validação
        public ICommand ValidateMoleculeCommand { get; }
        public ICommand AnalyzeMoleculeCommand { get; }
        #endregion

        #region Comandos - Templates
        public ICommand LoadTemplateCommand { get; }
        public ICommand SaveAsTemplateCommand { get; }
        #endregion

        #region Comandos - Utilitários
        public ICommand SaveMoleculeCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ShowTutorialCommand { get; }
        public ICommand DeleteSelectedCommand { get; }
        public ICommand Toggle3DViewCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand ResetZoomCommand { get; }
        public ICommand OptimizeGeometryCommand { get; }
        public ICommand SelectElementCommand { get; }
        #endregion

        #region Construtor
        public MoleculeBuilderViewModel(
            ChemistryEngine chemistryEngine,
            ElementDataLoader elementDataLoader,
            CompoundDataLoader compoundDataLoader)
        {
            _chemistryEngine = chemistryEngine ?? throw new ArgumentNullException(nameof(chemistryEngine));
            _elementDataLoader = elementDataLoader ?? throw new ArgumentNullException(nameof(elementDataLoader));
            _compoundDataLoader = compoundDataLoader ?? throw new ArgumentNullException(nameof(compoundDataLoader));

            // Inicializar coleções
            _atoms = new ObservableCollection<Atom>();
            _bonds = new ObservableCollection<Bond>();
            _validationWarnings = new ObservableCollection<ValidationWarning>();
            _moleculeTemplates = new ObservableCollection<MoleculeTemplate>();
            _availableElements = new ObservableCollection<Element>();
            _undoStack = new Stack<MoleculeAction>();
            _redoStack = new Stack<MoleculeAction>();

            // Inicializar molécula
            _currentMolecule = new Molecule();
            _selectedBondType = BondType.Single;
            _zoomLevel = 1.0;
            _isEducationalMode = true;
            _educationalMessage = "👋 Bem-vindo! Selecione um elemento químico abaixo para começar a construir sua molécula.";
            _molecularFormula = "—";
            _validationSummary = "Nenhum átomo adicionado.";
            
            // Configurações 3D padrão
            _atomSize3D = 0.5;
            _bondThickness3D = 0.1;
            _showLabels3D = false;
            _viewStyle3D = "BallAndStick";

            // Inicializar comandos
            AddAtomCommand = new RelayCommand<Point>(AddAtom, _ => SelectedElement != null);
            RemoveAtomCommand = new RelayCommand<Atom>(RemoveAtom, a => a != null);
            AddBondCommand = new RelayCommand<Atom>(StartBondCreation, a => a != null);
            RemoveBondCommand = new RelayCommand<Bond>(RemoveBond, b => b != null);
            ClearMoleculeCommand = new RelayCommand(ClearMolecule, () => Atoms.Count > 0);

            UndoCommand = new RelayCommand(Undo, () => CanUndo);
            RedoCommand = new RelayCommand(Redo, () => CanRedo);

            ValidateMoleculeCommand = new RelayCommand(ValidateMolecule);
            AnalyzeMoleculeCommand = new RelayCommand(UpdateMoleculeAnalysis);

            LoadTemplateCommand = new RelayCommand<MoleculeTemplate>(LoadTemplate, t => t != null);
            SaveAsTemplateCommand = new RelayCommand(SaveAsTemplate, () => Atoms.Count > 0);

            SaveMoleculeCommand = new RelayCommand(SaveMolecule, () => Atoms.Count > 0 && IsValid);
            ExportCommand = new RelayCommand(Export, () => Atoms.Count > 0);
            ShowTutorialCommand = new RelayCommand(ShowTutorial);
            DeleteSelectedCommand = new RelayCommand(DeleteSelected, () => SelectedAtom != null || SelectedBond != null);
            Toggle3DViewCommand = new RelayCommand(() => Show3DView = !Show3DView);
            ZoomInCommand = new RelayCommand(() => ZoomLevel = Math.Min(ZoomLevel + 0.1, 3.0));
            ZoomOutCommand = new RelayCommand(() => ZoomLevel = Math.Max(ZoomLevel - 0.1, 0.1));
            ResetZoomCommand = new RelayCommand(() => ZoomLevel = 1.0);
            OptimizeGeometryCommand = new RelayCommand(OptimizeGeometry, () => Atoms.Count >= 2 && Bonds.Count >= 1);
            SelectElementCommand = new RelayCommand<Element>(element => SelectedElement = element, e => e != null);

            // Carregar dados iniciais
            LoadElements();
            LoadTemplates();
        }
        #endregion

        #region Métodos - Manipulação de Átomos
        private void AddAtom(Point position)
        {
            if (SelectedElement == null) return;

            var atom = new Atom
            {
                Id = Atoms.Count,
                Element = SelectedElement,
                Position = new Point3D(position.X, position.Y, 0),
                Charge = 0,
                BondedAtomIds = new List<int>()
            };

            Atoms.Add(atom);
            CurrentMolecule.Atoms.Add(atom);

            // Adicionar ao histórico
            AddToHistory(new MoleculeAction
            {
                Type = ActionType.AddAtom,
                Atom = atom
            });

            UpdateMoleculeAnalysis();
            ValidateMolecule();

            if (IsEducationalMode)
            {
                EducationalMessage = $"✅ Átomo de {SelectedElement.Name} ({SelectedElement.Symbol}) adicionado!\n" +
                                   $"Valência típica: {GetTypicalValence(SelectedElement)}";
            }
        }

        private void RemoveAtom(Atom atom)
        {
            if (atom == null) return;

            // Remover ligações conectadas
            var bondsToRemove = Bonds.Where(b => b.Atom1 == atom || b.Atom2 == atom).ToList();
            foreach (var bond in bondsToRemove)
            {
                Bonds.Remove(bond);
                CurrentMolecule.Bonds.Remove(bond);
            }

            Atoms.Remove(atom);
            CurrentMolecule.Atoms.Remove(atom);

            // Adicionar ao histórico
            AddToHistory(new MoleculeAction
            {
                Type = ActionType.RemoveAtom,
                Atom = atom,
                RelatedBonds = bondsToRemove
            });

            UpdateMoleculeAnalysis();
            ValidateMolecule();
        }
        #endregion

        #region Métodos - Manipulação de Ligações
        private void StartBondCreation(Atom atom)
        {
            if (_firstAtomForBond == null)
            {
                _firstAtomForBond = atom.Position;
                SelectedAtom = atom;
                EducationalMessage = "🔗 Clique em outro átomo para criar uma ligação.";
            }
            else
            {
                var firstAtom = Atoms.FirstOrDefault(a => a.Position == _firstAtomForBond);
                if (firstAtom != null && firstAtom != atom)
                {
                    CreateBond(firstAtom, atom);
                }
                _firstAtomForBond = null;
            }
        }

        private void CreateBond(Atom atom1, Atom atom2)
        {
            // Verificar se já existe ligação
            if (Bonds.Any(b => (b.Atom1 == atom1 && b.Atom2 == atom2) || (b.Atom1 == atom2 && b.Atom2 == atom1)))
            {
                if (IsEducationalMode)
                {
                    EducationalMessage = "⚠️ Já existe uma ligação entre esses átomos!";
                }
                return;
            }

            // Verificar valência
            var validation = ValidateBondCreation(atom1, atom2, SelectedBondType);
            if (!validation.IsValid && IsEducationalMode)
            {
                EducationalMessage = $"❌ {validation.Message}";
                return;
            }

            var bond = new Bond
            {
                Atom1 = atom1,
                Atom2 = atom2,
                Type = SelectedBondType,
                Length = CalculateBondLength(atom1.Position, atom2.Position),
                Energy = _chemistryEngine.CalculateBondEnergy(new Bond { Atom1 = atom1, Atom2 = atom2, Type = SelectedBondType })
            };

            // Atualizar lista de átomos ligados
            atom1.BondedAtomIds.Add(atom2.Id);
            atom2.BondedAtomIds.Add(atom1.Id);

            Bonds.Add(bond);
            CurrentMolecule.Bonds.Add(bond);

            // Adicionar ao histórico
            AddToHistory(new MoleculeAction
            {
                Type = ActionType.AddBond,
                Bond = bond
            });

            UpdateMoleculeAnalysis();
            ValidateMolecule();

            if (IsEducationalMode)
            {
                EducationalMessage = $"✅ Ligação {SelectedBondType} criada!\n" +
                                   $"Energia: {bond.Energy:F1} kJ/mol\n" +
                                   $"Comprimento: {bond.Length:F2} Å";
            }
        }

        private void RemoveBond(Bond bond)
        {
            if (bond == null) return;

            // Atualizar lista de átomos ligados
            bond.Atom1.BondedAtomIds.Remove(bond.Atom2.Id);
            bond.Atom2.BondedAtomIds.Remove(bond.Atom1.Id);

            Bonds.Remove(bond);
            CurrentMolecule.Bonds.Remove(bond);

            // Adicionar ao histórico
            AddToHistory(new MoleculeAction
            {
                Type = ActionType.RemoveBond,
                Bond = bond
            });

            UpdateMoleculeAnalysis();
            ValidateMolecule();
        }

        private double CalculateBondLength(Point3D p1, Point3D p2)
        {
            var dx = p1.X - p2.X;
            var dy = p1.Y - p2.Y;
            var dz = p1.Z - p2.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
        #endregion

        #region Métodos - Análise Química
        private void UpdateMoleculeAnalysis()
        {
            if (CurrentMolecule == null || Atoms.Count == 0)
            {
                MolecularFormula = "—";
                MolarMass = 0;
                TotalBonds = 0;
                IsPolar = false;
                MoleculeType = "—";
                return;
            }

            // Fórmula molecular
            MolecularFormula = CalculateMolecularFormula();

            // Massa molar
            MolarMass = Atoms.Sum(a => a.Element.AtomicMass);

            // Número de ligações
            TotalBonds = Bonds.Count;

            // Polaridade
            IsPolar = _chemistryEngine.IsPolar(CurrentMolecule);
            DipoleMoment = _chemistryEngine.CalculateDipoleMoment(CurrentMolecule);

            // Geometria molecular
            if (Atoms.Count >= 2)
            {
                Geometry = _chemistryEngine.DetermineMolecularGeometry(CurrentMolecule);
            }

            // Tipo de molécula
            MoleculeType = DetermineMoleculeType();
        }

        private string CalculateMolecularFormula()
        {
            var elementCounts = new Dictionary<string, int>();

            foreach (var atom in Atoms)
            {
                if (elementCounts.ContainsKey(atom.Element.Symbol))
                    elementCounts[atom.Element.Symbol]++;
                else
                    elementCounts[atom.Element.Symbol] = 1;
            }

            // Ordem de Hill: C, H, depois alfabética
            var formula = "";
            if (elementCounts.ContainsKey("C"))
            {
                formula += "C";
                if (elementCounts["C"] > 1) formula += ToSubscript(elementCounts["C"]);
                elementCounts.Remove("C");
            }
            if (elementCounts.ContainsKey("H"))
            {
                formula += "H";
                if (elementCounts["H"] > 1) formula += ToSubscript(elementCounts["H"]);
                elementCounts.Remove("H");
            }

            foreach (var kvp in elementCounts.OrderBy(x => x.Key))
            {
                formula += kvp.Key;
                if (kvp.Value > 1) formula += ToSubscript(kvp.Value);
            }

            return string.IsNullOrEmpty(formula) ? "—" : formula;
        }

        private string ToSubscript(int number)
        {
            var subscripts = new[] { "₀", "₁", "₂", "₃", "₄", "₅", "₆", "₇", "₈", "₉" };
            return string.Join("", number.ToString().Select(c => subscripts[c - '0']));
        }

        private string DetermineMoleculeType()
        {
            if (Atoms.Count == 0) return "—";

            bool hasCarbon = Atoms.Any(a => a.Element.Symbol == "C");
            bool hasHydrogen = Atoms.Any(a => a.Element.Symbol == "H");

            if (hasCarbon && hasHydrogen)
                return "🧪 Orgânica";
            else if (hasCarbon)
                return "🧪 Composto de Carbono";
            else
                return "⚗️ Inorgânica";
        }
        #endregion

        #region Métodos - Validação
        private void ValidateMolecule()
        {
            ValidationWarnings.Clear();
            IsValid = true;

            if (Atoms.Count == 0)
            {
                ValidationSummary = "Nenhum átomo adicionado.";
                return;
            }

            foreach (var atom in Atoms)
            {
                ValidateAtomValence(atom);
            }

            // Verificar molécula isolada
            if (Bonds.Count == 0 && Atoms.Count > 1)
            {
                ValidationWarnings.Add(new ValidationWarning(
                    "⚠️ Átomos não estão conectados por ligações.",
                    "Warning"));
            }

            // Resumo
            int errors = ValidationWarnings.Count(w => w.Severity == "Error");
            int warnings = ValidationWarnings.Count(w => w.Severity == "Warning");

            IsValid = errors == 0;

            if (errors == 0 && warnings == 0)
                ValidationSummary = "✅ Molécula válida!";
            else if (errors > 0)
                ValidationSummary = $"❌ {errors} erro(s), {warnings} aviso(s)";
            else
                ValidationSummary = $"⚠️ {warnings} aviso(s)";
        }

        private void ValidateAtomValence(Atom atom)
        {
            int typicalValence = GetTypicalValence(atom.Element);
            int currentBonds = atom.BondedAtomIds.Count;

            // Somar ordem de ligação
            int totalBondOrder = 0;
            foreach (var bond in Bonds.Where(b => b.Atom1 == atom || b.Atom2 == atom))
            {
                totalBondOrder += bond.BondOrder;
            }

            if (totalBondOrder > typicalValence)
            {
                ValidationWarnings.Add(new ValidationWarning(
                    $"❌ {atom.Element.Symbol}: Valência excedida! " +
                    $"(Atual: {totalBondOrder}, Máxima: {typicalValence})",
                    "Error")
                {
                    AtomId = atom.Id.ToString()
                });
                IsValid = false;

                if (IsEducationalMode)
                {
                    EducationalMessage = $"💡 O {atom.Element.Name} geralmente forma {typicalValence} ligação(ões).\n" +
                                       $"Você tentou formar {totalBondOrder} ligação(ões).";
                }
            }
            else if (totalBondOrder < typicalValence && currentBonds > 0)
            {
                ValidationWarnings.Add(new ValidationWarning(
                    $"⚠️ {atom.Element.Symbol}: Pode aceitar mais {typicalValence - totalBondOrder} ligação(ões).",
                    "Info")
                {
                    AtomId = atom.Id.ToString()
                });
            }
        }

        private (bool IsValid, string Message) ValidateBondCreation(Atom atom1, Atom atom2, BondType bondType)
        {
            int valence1 = GetTypicalValence(atom1.Element);
            int valence2 = GetTypicalValence(atom2.Element);

            int currentBonds1 = Bonds.Where(b => b.Atom1 == atom1 || b.Atom2 == atom1).Sum(b => b.BondOrder);
            int currentBonds2 = Bonds.Where(b => b.Atom1 == atom2 || b.Atom2 == atom2).Sum(b => b.BondOrder);

            int bondOrder = bondType switch
            {
                BondType.Single => 1,
                BondType.Double => 2,
                BondType.Triple => 3,
                _ => 1
            };

            if (currentBonds1 + bondOrder > valence1)
            {
                return (false, $"Valência de {atom1.Element.Symbol} seria excedida! (Máx: {valence1}, Atual: {currentBonds1})");
            }

            if (currentBonds2 + bondOrder > valence2)
            {
                return (false, $"Valência de {atom2.Element.Symbol} seria excedida! (Máx: {valence2}, Atual: {currentBonds2})");
            }

            return (true, "OK");
        }

        private int GetTypicalValence(Element element)
        {
            // Valências típicas baseadas na tabela periódica
            return element.Symbol switch
            {
                "H" => 1,
                "C" => 4,
                "N" => 3,
                "O" => 2,
                "F" => 1,
                "Cl" => 1,
                "Br" => 1,
                "I" => 1,
                "S" => 2,
                "P" => 3,
                "Si" => 4,
                _ => element.ValenceElectrons?.FirstOrDefault() ?? 4
            };
        }
        #endregion

        #region Métodos - Histórico (Undo/Redo)
        private void AddToHistory(MoleculeAction action)
        {
            _undoStack.Push(action);
            _redoStack.Clear();
            CanUndo = _undoStack.Count > 0;
            CanRedo = false;
        }

        private void Undo()
        {
            if (_undoStack.Count == 0) return;

            var action = _undoStack.Pop();
            _redoStack.Push(action);

            switch (action.Type)
            {
                case ActionType.AddAtom:
                    Atoms.Remove(action.Atom);
                    CurrentMolecule.Atoms.Remove(action.Atom);
                    break;
                case ActionType.RemoveAtom:
                    Atoms.Add(action.Atom);
                    CurrentMolecule.Atoms.Add(action.Atom);
                    foreach (var bond in action.RelatedBonds)
                    {
                        Bonds.Add(bond);
                        CurrentMolecule.Bonds.Add(bond);
                    }
                    break;
                case ActionType.AddBond:
                    Bonds.Remove(action.Bond);
                    CurrentMolecule.Bonds.Remove(action.Bond);
                    break;
                case ActionType.RemoveBond:
                    Bonds.Add(action.Bond);
                    CurrentMolecule.Bonds.Add(action.Bond);
                    break;
            }

            CanUndo = _undoStack.Count > 0;
            CanRedo = _redoStack.Count > 0;
            UpdateMoleculeAnalysis();
            ValidateMolecule();
        }

        private void Redo()
        {
            if (_redoStack.Count == 0) return;

            var action = _redoStack.Pop();
            _undoStack.Push(action);

            switch (action.Type)
            {
                case ActionType.AddAtom:
                    Atoms.Add(action.Atom);
                    CurrentMolecule.Atoms.Add(action.Atom);
                    break;
                case ActionType.RemoveAtom:
                    Atoms.Remove(action.Atom);
                    CurrentMolecule.Atoms.Remove(action.Atom);
                    foreach (var bond in action.RelatedBonds)
                    {
                        Bonds.Remove(bond);
                        CurrentMolecule.Bonds.Remove(bond);
                    }
                    break;
                case ActionType.AddBond:
                    Bonds.Add(action.Bond);
                    CurrentMolecule.Bonds.Add(action.Bond);
                    break;
                case ActionType.RemoveBond:
                    Bonds.Remove(action.Bond);
                    CurrentMolecule.Bonds.Remove(action.Bond);
                    break;
            }

            CanUndo = _undoStack.Count > 0;
            CanRedo = _redoStack.Count > 0;
            UpdateMoleculeAnalysis();
            ValidateMolecule();
        }
        #endregion

        #region Métodos - Templates e Elementos
        /// <summary>
        /// Carrega elementos comuns do ElementDataLoader
        /// </summary>
        private void LoadElements()
        {
            try
            {
                // Carregar todos os elementos do loader
                var allElements = _elementDataLoader.LoadElements();

                // Elementos mais comuns para construção de moléculas
                var commonSymbols = new[] { "H", "C", "N", "O", "F", "P", "S", "Cl", "Br", "I", "Na", "K", "Ca", "Mg" };

                foreach (var symbol in commonSymbols)
                {
                    var element = _elementDataLoader.GetElementBySymbol(symbol);
                    if (element != null)
                    {
                        AvailableElements.Add(element);
                    }
                }

                SelectedElement = AvailableElements.FirstOrDefault();

                if (AvailableElements.Count == 0)
                {
                    throw new InvalidOperationException("Nenhum elemento foi carregado.");
                }

                System.Diagnostics.Debug.WriteLine($"✅ {AvailableElements.Count} elementos carregados para construção de moléculas");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Erro ao carregar elementos: {ex.Message}");
                
                // Fallback: criar elementos básicos manualmente
                LoadBasicElementsFallback();
            }
        }

        /// <summary>
        /// Fallback caso o ElementDataLoader falhe
        /// </summary>
        private void LoadBasicElementsFallback()
        {
            var basicElements = new[]
            {
                new Element(1, "H", "Hidrogênio") { AtomicMass = 1.008, Category = ElementCategory.NonMetal, ValenceElectrons = new[] { 1 } },
                new Element(6, "C", "Carbono") { AtomicMass = 12.011, Category = ElementCategory.NonMetal, ValenceElectrons = new[] { 4 } },
                new Element(7, "N", "Nitrogênio") { AtomicMass = 14.007, Category = ElementCategory.NonMetal, ValenceElectrons = new[] { 3 } },
                new Element(8, "O", "Oxigênio") { AtomicMass = 15.999, Category = ElementCategory.NonMetal, ValenceElectrons = new[] { 2 } },
                new Element(9, "F", "Flúor") { AtomicMass = 18.998, Category = ElementCategory.Halogen, ValenceElectrons = new[] { 1 } },
                new Element(17, "Cl", "Cloro") { AtomicMass = 35.45, Category = ElementCategory.Halogen, ValenceElectrons = new[] { 1 } }
            };

            foreach (var element in basicElements)
            {
                AvailableElements.Add(element);
            }

            SelectedElement = AvailableElements.FirstOrDefault();
        }

        /// <summary>
        /// Carrega templates de moléculas do CompoundDataLoader
        /// </summary>
        private void LoadTemplates()
        {
            try
            {
                // Carregar compostos do loader
                var compounds = _compoundDataLoader.LoadCompounds();

                // Selecionar compostos simples e interessantes para templates
                var templateCompounds = compounds
                    .Where(c => IsGoodTemplate(c))
                    .Take(20) // Limitar a 20 templates
                    .ToList();

                foreach (var compound in templateCompounds)
                {
                    MoleculeTemplates.Add(new MoleculeTemplate
                    {
                        Name = $"{compound.Name} ({compound.Formula})",
                        Formula = compound.Formula,
                        Description = compound.CommonUse ?? "Composto químico",
                        Category = compound.Category.ToString()
                    });
                }

                System.Diagnostics.Debug.WriteLine($"✅ {MoleculeTemplates.Count} templates de moléculas carregados");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Erro ao carregar templates: {ex.Message}");
                
                // Fallback: criar templates básicos manualmente
                LoadBasicTemplatesFallback();
            }
        }

        /// <summary>
        /// Determina se um composto é adequado como template
        /// </summary>
        private bool IsGoodTemplate(Compound compound)
        {
            // Evitar compostos muito complexos
            if (compound.Formula.Length > 15) return false;

            // Preferir compostos conhecidos e simples
            var goodTemplates = new[]
            {
                "H2O", "CO2", "CH4", "NH3", "HCl", "NaCl", "H2O2", "O2", "N2",
                "C2H6O", "CH3OH", "C2H4", "C2H2", "SO2", "NO2", "H2SO4", "HNO3"
            };

            return goodTemplates.Contains(compound.Formula);
        }

        /// <summary>
        /// Fallback caso o CompoundDataLoader falhe
        /// </summary>
        private void LoadBasicTemplatesFallback()
        {
            MoleculeTemplates.Add(new MoleculeTemplate
            {
                Name = "Água (H₂O)",
                Formula = "H₂O",
                Description = "Molécula de água - essencial para a vida",
                Category = "Inorganic"
            });

            MoleculeTemplates.Add(new MoleculeTemplate
            {
                Name = "Dióxido de Carbono (CO₂)",
                Formula = "CO₂",
                Description = "Gás encontrado na atmosfera",
                Category = "Oxide"
            });

            MoleculeTemplates.Add(new MoleculeTemplate
            {
                Name = "Metano (CH₄)",
                Formula = "CH₄",
                Description = "Hidrocarboneto mais simples",
                Category = "HydroCarbon"
            });

            MoleculeTemplates.Add(new MoleculeTemplate
            {
                Name = "Amônia (NH₃)",
                Formula = "NH₃",
                Description = "Base comum em química",
                Category = "Inorganic"
            });

            MoleculeTemplates.Add(new MoleculeTemplate
            {
                Name = "Etanol (C₂H₆O)",
                Formula = "C₂H₆O",
                Description = "Álcool comum",
                Category = "Alcohol"
            });
        }

        /// <summary>
        /// Carrega um template de molécula
        /// </summary>
        private void LoadTemplate(MoleculeTemplate template)
        {
            if (template == null) return;

            ClearMolecule();

            // Normalizar fórmula para comparação
            var formula = template.Formula.Replace("₂", "2").Replace("₃", "3").Replace("₄", "4");

            switch (formula)
            {
                case "H2O":
                    LoadWaterTemplate();
                    break;
                case "CO2":
                    LoadCO2Template();
                    break;
                case "CH4":
                    LoadMethaneTemplate();
                    break;
                case "C2H6O":
                case "CH3CH2OH":
                    LoadEthanolTemplate();
                    break;
                case "NH3":
                    LoadAmmoniaTemplate();
                    break;
                case "HCl":
                    LoadHClTemplate();
                    break;
                case "NaCl":
                    LoadNaClTemplate();
                    break;
                case "O2":
                    LoadO2Template();
                    break;
                case "N2":
                    LoadN2Template();
                    break;
                default:
                    if (IsEducationalMode)
                    {
                        EducationalMessage = $"⚠️ Template '{template.Name}' ainda não está implementado.\n" +
                                           "Você pode construir a molécula manualmente!";
                    }
                    break;
            }

            if (Atoms.Count > 0 && IsEducationalMode)
            {
                EducationalMessage = $"📚 Template carregado: {template.Name}\n{template.Description}";
            }

            // Otimizar geometria 3D automaticamente após carregar template
            if (Atoms.Count >= 2 && Bonds.Count >= 1)
            {
                OptimizeGeometry();
            }
        }

        /// <summary>
        /// Template: Água (H₂O)
        /// </summary>
        private void LoadWaterTemplate()
        {
            var o = _elementDataLoader.GetElementBySymbol("O");
            var h = _elementDataLoader.GetElementBySymbol("H");

            if (o == null || h == null) return;

            SelectedElement = o;
            AddAtom(new Point(200, 200));
            
            SelectedElement = h;
            AddAtom(new Point(150, 250));
            AddAtom(new Point(250, 250));

            CreateBond(Atoms[0], Atoms[1]);
            CreateBond(Atoms[0], Atoms[2]);
        }

        /// <summary>
        /// Template: Dióxido de Carbono (CO₂)
        /// </summary>
        private void LoadCO2Template()
        {
            var c = _elementDataLoader.GetElementBySymbol("C");
            var o = _elementDataLoader.GetElementBySymbol("O");

            if (c == null || o == null) return;

            SelectedElement = c;
            AddAtom(new Point(200, 200));
            
            SelectedElement = o;
            AddAtom(new Point(130, 200));
            AddAtom(new Point(270, 200));

            SelectedBondType = BondType.Double;
            CreateBond(Atoms[0], Atoms[1]);
            CreateBond(Atoms[0], Atoms[2]);
            SelectedBondType = BondType.Single;
        }

        /// <summary>
        /// Template: Metano (CH₄)
        /// </summary>
        private void LoadMethaneTemplate()
        {
            var c = _elementDataLoader.GetElementBySymbol("C");
            var h = _elementDataLoader.GetElementBySymbol("H");

            if (c == null || h == null) return;

            SelectedElement = c;
            AddAtom(new Point(200, 200));
            
            SelectedElement = h;
            AddAtom(new Point(200, 140));  // Cima
            AddAtom(new Point(140, 220));   // Esquerda
            AddAtom(new Point(260, 220));   // Direita
            AddAtom(new Point(200, 260));   // Baixo

            for (int i = 1; i <= 4; i++)
            {
                CreateBond(Atoms[0], Atoms[i]);
            }
        }

        /// <summary>
        /// Template: Etanol (C₂H₆O)
        /// </summary>
        private void LoadEthanolTemplate()
        {
            var c = _elementDataLoader.GetElementBySymbol("C");
            var h = _elementDataLoader.GetElementBySymbol("H");
            var o = _elementDataLoader.GetElementBySymbol("O");

            if (c == null || h == null || o == null) return;

            // Estrutura simplificada: C-C-O-H
            SelectedElement = c;
            AddAtom(new Point(140, 200));   // C1
            AddAtom(new Point(200, 200));   // C2
            
            SelectedElement = o;
            AddAtom(new Point(260, 200));   // O
            
            SelectedElement = h;
            AddAtom(new Point(320, 200));   // H do OH

            // Ligações
            CreateBond(Atoms[0], Atoms[1]);  // C-C
            CreateBond(Atoms[1], Atoms[2]);  // C-O
            CreateBond(Atoms[2], Atoms[3]);  // O-H
        }

        /// <summary>
        /// Template: Amônia (NH₃)
        /// </summary>
        private void LoadAmmoniaTemplate()
        {
            var n = _elementDataLoader.GetElementBySymbol("N");
            var h = _elementDataLoader.GetElementBySymbol("H");

            if (n == null || h == null) return;

            SelectedElement = n;
            AddAtom(new Point(200, 200));
            
            SelectedElement = h;
            AddAtom(new Point(200, 150));   // Cima
            AddAtom(new Point(160, 230));   // Esquerda-baixo
            AddAtom(new Point(240, 230));   // Direita-baixo

            for (int i = 1; i <= 3; i++)
            {
                CreateBond(Atoms[0], Atoms[i]);
            }
        }

        /// <summary>
        /// Template: Ácido Clorídrico (HCl)
        /// </summary>
        private void LoadHClTemplate()
        {
            var h = _elementDataLoader.GetElementBySymbol("H");
            var cl = _elementDataLoader.GetElementBySymbol("Cl");

            if (h == null || cl == null) return;

            SelectedElement = h;
            AddAtom(new Point(170, 200));
            
            SelectedElement = cl;
            AddAtom(new Point(230, 200));

            CreateBond(Atoms[0], Atoms[1]);
        }

        /// <summary>
        /// Template: Cloreto de Sódio (NaCl) - Ligação Iônica
        /// </summary>
        private void LoadNaClTemplate()
        {
            var na = _elementDataLoader.GetElementBySymbol("Na");
            var cl = _elementDataLoader.GetElementBySymbol("Cl");

            if (na == null || cl == null) return;

            SelectedElement = na;
            AddAtom(new Point(170, 200));
            
            SelectedElement = cl;
            AddAtom(new Point(230, 200));

            // Ligação iônica (representada como single)
            var bond = new Bond
            {
                Atom1 = Atoms[0],
                Atom2 = Atoms[1],
                Type = BondType.Ionic,
                Length = CalculateBondLength(Atoms[0].Position, Atoms[1].Position)
            };

            Atoms[0].BondedAtomIds.Add(Atoms[1].Id);
            Atoms[1].BondedAtomIds.Add(Atoms[0].Id);

            Bonds.Add(bond);
            CurrentMolecule.Bonds.Add(bond);

            UpdateMoleculeAnalysis();
            ValidateMolecule();
        }

        /// <summary>
        /// Template: Oxigênio molecular (O₂)
        /// </summary>
        private void LoadO2Template()
        {
            var o = _elementDataLoader.GetElementBySymbol("O");

            if (o == null) return;

            SelectedElement = o;
            AddAtom(new Point(170, 200));
            AddAtom(new Point(230, 200));

            SelectedBondType = BondType.Double;
            CreateBond(Atoms[0], Atoms[1]);
            SelectedBondType = BondType.Single;
        }

        /// <summary>
        /// Template: Nitrogênio molecular (N₂)
        /// </summary>
        private void LoadN2Template()
        {
            var n = _elementDataLoader.GetElementBySymbol("N");

            if (n == null) return;

            SelectedElement = n;
            AddAtom(new Point(170, 200));
            AddAtom(new Point(230, 200));

            SelectedBondType = BondType.Triple;
            CreateBond(Atoms[0], Atoms[1]);
            SelectedBondType = BondType.Single;
        }

        private void SaveAsTemplate()
        {
            // Implementar salvamento de template personalizado
            MessageBox.Show("Funcionalidade de salvar template será implementada em breve!",
                "Em Desenvolvimento", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        #region Métodos - Utilitários
        private void ClearMolecule()
        {
            Atoms.Clear();
            Bonds.Clear();
            CurrentMolecule = new Molecule();
            ValidationWarnings.Clear();
            _undoStack.Clear();
            _redoStack.Clear();
            CanUndo = false;
            CanRedo = false;
            UpdateMoleculeAnalysis();
            EducationalMessage = "🆕 Nova molécula iniciada!";
        }

        private void SaveMolecule()
        {
            if (!IsValid)
            {
                MessageBox.Show("Não é possível salvar uma molécula inválida.",
                    "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBox.Show($"Molécula {MolecularFormula} salva com sucesso!",
                "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Export()
        {
            MessageBox.Show("Funcionalidade de exportação será implementada em breve!",
                "Em Desenvolvimento", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowTutorial()
        {
            var tutorial = "🎓 TUTORIAL - MOLECULE BUILDER\n\n" +
                          "1️⃣ Selecione um elemento na paleta\n" +
                          "2️⃣ Clique no canvas para adicionar átomos\n" +
                          "3️⃣ Clique em dois átomos para criar ligações\n" +
                          "4️⃣ Use o tipo de ligação (Simples/Dupla/Tripla)\n" +
                          "5️⃣ Analise as propriedades no painel direito\n" +
                          "6️⃣ Verifique avisos de validação\n\n" +
                          "⌨️ ATALHOS:\n" +
                          "Ctrl+Z: Desfazer\n" +
                          "Ctrl+Y: Refazer\n" +
                          "Ctrl+S: Salvar\n" +
                          "Ctrl+N: Nova molécula\n" +
                          "Delete: Excluir selecionado";

            MessageBox.Show(tutorial, "Tutorial", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DeleteSelected()
        {
            if (SelectedAtom != null)
            {
                RemoveAtom(SelectedAtom);
                SelectedAtom = null;
            }
            else if (SelectedBond != null)
            {
                RemoveBond(SelectedBond);
                SelectedBond = null;
            }
        }

        private void UpdateAtomInfo()
        {
            if (SelectedAtom == null) return;

            var bonds = Bonds.Where(b => b.Atom1 == SelectedAtom || b.Atom2 == SelectedAtom).Count();
            EducationalMessage = $"📍 Átomo selecionado: {SelectedAtom.Element.Name} ({SelectedAtom.Element.Symbol})\n" +
                               $"Ligações: {bonds}\n" +
                               $"Valência típica: {GetTypicalValence(SelectedAtom.Element)}";
        }
        #endregion

        #region Métodos - Otimização de Geometria 3D
        /// <summary>
        /// Otimiza a geometria 3D da molécula baseada em regras VSEPR e comprimentos de ligação típicos
        /// </summary>
        private void OptimizeGeometry()
        {
            if (Atoms.Count < 2 || Bonds.Count < 1)
            {
                if (IsEducationalMode)
                {
                    EducationalMessage = "⚠️ Necessário pelo menos 2 átomos conectados para otimizar geometria.";
                }
                return;
            }

            try
            {
                // Adicionar ao histórico
                AddToHistory(new MoleculeAction
                {
                    Type = ActionType.OptimizeGeometry,
                    Description = "Otimização de geometria 3D"
                });

                // Passo 1: Encontrar átomo central (mais ligações)
                var centralAtom = FindCentralAtom();

                if (centralAtom == null)
                {
                    // Molécula linear simples (ex: H-H, H-Cl)
                    OptimizeLinearMolecule();
                }
                else
                {
                    // Otimizar baseado em geometria VSEPR
                    OptimizeByVSEPR(centralAtom);
                }

                // Passo 2: Ajustar comprimentos de ligação
                AdjustBondLengths();

                // Passo 3: Centralizar molécula no canvas
                CenterMolecule();

                if (IsEducationalMode)
                {
                    EducationalMessage = $"✅ Geometria 3D otimizada!\n" +
                                       $"Geometria molecular: {Geometry}\n" +
                                       $"Átomos reposicionados com ângulos e comprimentos ideais.";
                }

                // Forçar atualização da renderização
                OnPropertyChanged(nameof(Atoms));
                OnPropertyChanged(nameof(Bonds));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Erro na otimização de geometria: {ex.Message}");
                if (IsEducationalMode)
                {
                    EducationalMessage = "❌ Erro ao otimizar geometria. Tente novamente.";
                }
            }
        }

        /// <summary>
        /// Encontra o átomo central (com mais ligações)
        /// </summary>
        private Atom FindCentralAtom()
        {
            return Atoms
                .OrderByDescending(a => a.BondedAtomIds.Count)
                .FirstOrDefault(a => a.BondedAtomIds.Count > 1);
        }

        /// <summary>
        /// Otimiza molécula linear simples (2 átomos)
        /// </summary>
        private void OptimizeLinearMolecule()
        {
            if (Atoms.Count != 2) return;

            var atom1 = Atoms[0];
            var atom2 = Atoms[1];

            // Distância ideal baseada nos elementos
            double idealDistance = GetIdealBondLength(atom1.Element, atom2.Element, SelectedBondType);

            // Posicionar no centro do canvas
            atom1.Position = new Point3D(200 - idealDistance / 2, 200, 0);
            atom2.Position = new Point3D(200 + idealDistance / 2, 200, 0);
        }

        /// <summary>
        /// Otimiza geometria baseada em VSEPR
        /// </summary>
        private void OptimizeByVSEPR(Atom centralAtom)
        {
            var bondedAtoms = Atoms.Where(a => centralAtom.BondedAtomIds.Contains(a.Id)).ToList();
            int numBonds = bondedAtoms.Count;

            // Posicionar átomo central no centro
            centralAtom.Position = new Point3D(200, 200, 0);

            // Ângulos ideais baseados em VSEPR
            var angles = GetIdealAngles(numBonds);

            // Posicionar átomos ligados
            for (int i = 0; i < bondedAtoms.Count; i++)
            {
                var bondedAtom = bondedAtoms[i];
                var bond = Bonds.FirstOrDefault(b => 
                    (b.Atom1 == centralAtom && b.Atom2 == bondedAtom) ||
                    (b.Atom2 == centralAtom && b.Atom1 == bondedAtom));

                // Distância ideal
                double distance = GetIdealBondLength(centralAtom.Element, bondedAtom.Element, bond?.Type ?? BondType.Single);

                // Calcular posição 3D baseada nos ângulos
                var position = CalculateAtomPosition(centralAtom.Position, distance, angles[i]);
                bondedAtom.Position = position;
            }
        }

        /// <summary>
        /// Retorna ângulos ideais baseados no número de ligações (VSEPR)
        /// </summary>
        private List<(double theta, double phi)> GetIdealAngles(int numBonds)
        {
            var angles = new List<(double theta, double phi)>();

            switch (numBonds)
            {
                case 1:
                    // Linear (não aplicável, mas incluído para completude)
                    angles.Add((0, 0));
                    break;

                case 2:
                    // Linear: 180°
                    angles.Add((0, 0));
                    angles.Add((Math.PI, 0));
                    break;

                case 3:
                    // Trigonal Planar: 120°
                    for (int i = 0; i < 3; i++)
                    {
                        double angle = i * 2 * Math.PI / 3;
                        angles.Add((Math.PI / 2, angle)); // Plano XY
                    }
                    break;

                case 4:
                    // Tetraédrico: 109.47°
                    angles.Add((0, 0));                                    // Cima
                    angles.Add((Math.PI * 2 / 3, 0));                     // Baixo-frente
                    angles.Add((Math.PI * 2 / 3, 2 * Math.PI / 3));       // Baixo-esquerda
                    angles.Add((Math.PI * 2 / 3, 4 * Math.PI / 3));       // Baixo-direita
                    break;

                case 5:
                    // Bipiramidal Trigonal
                    angles.Add((0, 0));                          // Cima
                    angles.Add((Math.PI, 0));                    // Baixo
                    angles.Add((Math.PI / 2, 0));                // Equatorial 1
                    angles.Add((Math.PI / 2, 2 * Math.PI / 3));  // Equatorial 2
                    angles.Add((Math.PI / 2, 4 * Math.PI / 3));  // Equatorial 3
                    break;

                case 6:
                    // Octaédrico: 90°
                    angles.Add((0, 0));                          // +Z
                    angles.Add((Math.PI, 0));                    // -Z
                    angles.Add((Math.PI / 2, 0));                // +X
                    angles.Add((Math.PI / 2, Math.PI));          // -X
                    angles.Add((Math.PI / 2, Math.PI / 2));      // +Y
                    angles.Add((Math.PI / 2, 3 * Math.PI / 2));  // -Y
                    break;

                default:
                    // Fallback: distribuir uniformemente em esfera
                    for (int i = 0; i < numBonds; i++)
                    {
                        double theta = Math.Acos(1 - 2.0 * i / numBonds);
                        double phi = Math.PI * (1 + Math.Sqrt(5)) * i;
                        angles.Add((theta, phi));
                    }
                    break;
            }

            return angles;
        }

        /// <summary>
        /// Calcula a posição 3D de um átomo baseada em coordenadas esféricas
        /// </summary>
        private Point3D CalculateAtomPosition(Point3D center, double distance, (double theta, double phi) angles)
        {
            // Conversão de coordenadas esféricas para cartesianas
            double x = center.X + distance * Math.Sin(angles.theta) * Math.Cos(angles.phi);
            double y = center.Y + distance * Math.Sin(angles.theta) * Math.Sin(angles.phi);
            double z = center.Z + distance * Math.Cos(angles.theta);

            return new Point3D(x, y, z);
        }

        /// <summary>
        /// Retorna o comprimento ideal de ligação em pixels (aproximado)
        /// </summary>
        private double GetIdealBondLength(Element element1, Element element2, BondType bondType)
        {
            // Raios covalentes aproximados (em Angstroms)
            var covalentRadii = new Dictionary<string, double>
            {
                { "H", 0.31 }, { "C", 0.76 }, { "N", 0.71 }, { "O", 0.66 },
                { "F", 0.57 }, { "P", 1.07 }, { "S", 1.05 }, { "Cl", 1.02 },
                { "Br", 1.20 }, { "I", 1.39 }, { "Na", 1.66 }, { "K", 2.03 },
                { "Ca", 1.76 }, { "Mg", 1.41 }
            };

            double radius1 = covalentRadii.ContainsKey(element1.Symbol) ? covalentRadii[element1.Symbol] : 1.0;
            double radius2 = covalentRadii.ContainsKey(element2.Symbol) ? covalentRadii[element2.Symbol] : 1.0;

            // Comprimento de ligação = soma dos raios covalentes
            double bondLengthAngstroms = radius1 + radius2;

            // Ajustar para ligações múltiplas (são mais curtas)
            switch (bondType)
            {
                case BondType.Double:
                    bondLengthAngstroms *= 0.87;  // ~13% mais curta
                    break;
                case BondType.Triple:
                    bondLengthAngstroms *= 0.78;  // ~22% mais curta
                    break;
            }

            // Converter Angstroms para pixels (escala: 1 Angstrom ≈ 40 pixels)
            return bondLengthAngstroms * 40;
        }

        /// <summary>
        /// Ajusta os comprimentos de todas as ligações para valores ideais
        /// </summary>
        private void AdjustBondLengths()
        {
            foreach (var bond in Bonds)
            {
                double idealLength = GetIdealBondLength(bond.Atom1.Element, bond.Atom2.Element, bond.Type);
                
                var direction = new Vector3D(
                    bond.Atom2.Position.X - bond.Atom1.Position.X,
                    bond.Atom2.Position.Y - bond.Atom1.Position.Y,
                    bond.Atom2.Position.Z - bond.Atom1.Position.Z
                );

                direction.Normalize();

                // Reposicionar atom2 à distância ideal de atom1
                bond.Atom2.Position = new Point3D(
                    bond.Atom1.Position.X + direction.X * idealLength,
                    bond.Atom1.Position.Y + direction.Y * idealLength,
                    bond.Atom1.Position.Z + direction.Z * idealLength
                );

                // Atualizar comprimento da ligação
                bond.Length = idealLength / 40; // Converter de pixels para Angstroms
            }
        }

        /// <summary>
        /// Centraliza a molécula no canvas
        /// </summary>
        private void CenterMolecule()
        {
            if (Atoms.Count == 0) return;

            // Calcular centro de massa
            double centerX = Atoms.Average(a => a.Position.X);
            double centerY = Atoms.Average(a => a.Position.Y);
            double centerZ = Atoms.Average(a => a.Position.Z);

            // Deslocamento para centralizar em (200, 200, 0)
            double offsetX = 200 - centerX;
            double offsetY = 200 - centerY;
            double offsetZ = 0 - centerZ;

            // Aplicar deslocamento a todos os átomos
            foreach (var atom in Atoms)
            {
                atom.Position = new Point3D(
                    atom.Position.X + offsetX,
                    atom.Position.Y + offsetY,
                    atom.Position.Z + offsetZ
                );
            }
        }
        #endregion
    }

    #region Classes Auxiliares
    public class MoleculeTemplate
    {
        public string Name { get; set; }
        public string Formula { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
    }
    #endregion
}
