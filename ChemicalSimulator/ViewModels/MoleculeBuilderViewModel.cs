using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using ChemicalSimulator.Commands;
using ChemicalSimulator.Models;
using ChemicalSimulator.Services;

namespace ChemicalSimulator.ViewModels
{
    /// <summary>
    /// ViewModel para construção de moléculas
    /// </summary>
    public class MoleculeBuilderViewModel : ViewModelBase
    {
        private readonly ChemistryEngine _chemistryEngine;
        private readonly ObservableCollection<Molecule> _savedMolecules;

        private Molecule _currentMolecule;
        private Element? _selectedElement;
        private Atom? _selectedAtom1;
        private Atom? _selectedAtom2;
        private BondType _selectedBondType = BondType.Single;
        private string _moleculeName = "Nova Molécula";
        private string _searchText = "";

        public ObservableCollection<Element> AvailableElements { get; }
        public ObservableCollection<Element> FilteredElements { get; }
        public ObservableCollection<Atom> CurrentAtoms { get; }
        public ObservableCollection<Bond> CurrentBonds { get; }

        public Molecule CurrentMolecule
        {
            get => _currentMolecule;
            set => SetProperty(ref _currentMolecule, value);
        }

        public Element? SelectedElement
        {
            get => _selectedElement;
            set => SetProperty(ref _selectedElement, value);
        }

        public Atom? SelectedAtom1
        {
            get => _selectedAtom1;
            set => SetProperty(ref _selectedAtom1, value);
        }

        public Atom? SelectedAtom2
        {
            get => _selectedAtom2;
            set => SetProperty(ref _selectedAtom2, value);
        }

        public BondType SelectedBondType
        {
            get => _selectedBondType;
            set => SetProperty(ref _selectedBondType, value);
        }

        public string MoleculeName
        {
            get => _moleculeName;
            set => SetProperty(ref _moleculeName, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                FilterElements();
            }
        }

        public string MolecularFormula => CurrentMolecule?.GetMolecularFormula() ?? "";
        public double MolarMass => CurrentMolecule?.MolarMass ?? 0;
        public int TotalCharge => CurrentMolecule?.GetTotalCharge() ?? 0;

        // Comandos
        public ICommand AddAtomCommand { get; }
        public ICommand RemoveAtomCommand { get; }
        public ICommand AddBondCommand { get; }
        public ICommand RemoveBondCommand { get; }
        public ICommand ClearMoleculeCommand { get; }
        public ICommand SaveMoleculeCommand { get; }
        public ICommand OptimizeGeometryCommand { get; }
        public ICommand LoadTemplateCommand { get; }

        public MoleculeBuilderViewModel(
            ObservableCollection<Element> availableElements,
            ObservableCollection<Molecule> savedMolecules)
        {
            _chemistryEngine = new ChemistryEngine();
            _savedMolecules = savedMolecules;

            AvailableElements = availableElements;
            FilteredElements = new ObservableCollection<Element>(availableElements);
            CurrentAtoms = new ObservableCollection<Atom>();
            CurrentBonds = new ObservableCollection<Bond>();

            CurrentMolecule = new Molecule { Name = _moleculeName };

            // Inicializar comandos
            AddAtomCommand = new RelayCommand(AddAtom, CanAddAtom);
            RemoveAtomCommand = new RelayCommand<Atom>(RemoveAtom, CanRemoveAtom);
            AddBondCommand = new RelayCommand(AddBond, CanAddBond);
            RemoveBondCommand = new RelayCommand<Bond>(RemoveBond);
            ClearMoleculeCommand = new RelayCommand(ClearMolecule);
            SaveMoleculeCommand = new RelayCommand(SaveMolecule, CanSaveMolecule);
            OptimizeGeometryCommand = new RelayCommand(OptimizeGeometry);
            LoadTemplateCommand = new RelayCommand<string>(LoadTemplate);
        }

        private void FilterElements()
        {
            FilteredElements.Clear();
            
            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? AvailableElements
                : AvailableElements.Where(e =>
                    e.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    e.Symbol.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            foreach (var element in filtered)
            {
                FilteredElements.Add(element);
            }
        }

        private bool CanAddAtom() => SelectedElement != null;

        private void AddAtom()
        {
            if (SelectedElement == null) return;

            // Posição automática em grid 3D
            var index = CurrentAtoms.Count;
            var x = (index % 5) * 2.0;
            var y = (index / 5) * 2.0;
            var z = 0.0;

            CurrentMolecule.AddAtom(SelectedElement, new Point3D(x, y, z));
            
            var newAtom = CurrentMolecule.Atoms.Last();
            CurrentAtoms.Add(newAtom);

            UpdateMoleculeProperties();
        }

        private bool CanRemoveAtom(Atom? atom) => atom != null;

        private void RemoveAtom(Atom? atom)
        {
            if (atom == null) return;

            // Remover ligações associadas
            var bondsToRemove = CurrentMolecule.Bonds
                .Where(b => b.Atom1 == atom || b.Atom2 == atom)
                .ToList();

            foreach (var bond in bondsToRemove)
            {
                CurrentMolecule.Bonds.Remove(bond);
                CurrentBonds.Remove(bond);
            }

            CurrentMolecule.Atoms.Remove(atom);
            CurrentAtoms.Remove(atom);

            UpdateMoleculeProperties();
        }

        private bool CanAddBond() => SelectedAtom1 != null && SelectedAtom2 != null && SelectedAtom1 != SelectedAtom2;

        private void AddBond()
        {
            if (!CanAddBond()) return;

            CurrentMolecule.AddBond(SelectedAtom1!.Id, SelectedAtom2!.Id, SelectedBondType);
            
            var newBond = CurrentMolecule.Bonds.Last();
            CurrentBonds.Add(newBond);

            // Limpar seleção
            SelectedAtom1 = null;
            SelectedAtom2 = null;

            UpdateMoleculeProperties();
        }

        private void RemoveBond(Bond? bond)
        {
            if (bond == null) return;

            CurrentMolecule.Bonds.Remove(bond);
            CurrentBonds.Remove(bond);

            UpdateMoleculeProperties();
        }

        private void ClearMolecule()
        {
            CurrentAtoms.Clear();
            CurrentBonds.Clear();
            CurrentMolecule = new Molecule { Name = MoleculeName };
            
            SelectedAtom1 = null;
            SelectedAtom2 = null;

            UpdateMoleculeProperties();
        }

        private bool CanSaveMolecule() => CurrentMolecule?.Atoms.Count > 0;

        private void SaveMolecule()
        {
            CurrentMolecule.Name = MoleculeName;
            CurrentMolecule.CalculateMolarMass();
            CurrentMolecule.Formula = CurrentMolecule.GetMolecularFormula();
            CurrentMolecule.Geometry = _chemistryEngine.DetermineMolecularGeometry(CurrentMolecule);
            CurrentMolecule.IsPolar = _chemistryEngine.IsPolar(CurrentMolecule);

            _savedMolecules.Add(CurrentMolecule);

            MessageBox.Show($"Molécula '{MoleculeName}' salva com sucesso!", 
                "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

            ClearMolecule();
        }

        private void OptimizeGeometry()
        {
            // Otimização básica de geometria molecular
            var geometry = _chemistryEngine.DetermineMolecularGeometry(CurrentMolecule);
            CurrentMolecule.Geometry = geometry;

            // Reposicionar átomos baseado na geometria
            RepositionAtoms(geometry);

            UpdateMoleculeProperties();
        }

        private void RepositionAtoms(MoleculeGeometry geometry)
        {
            if (CurrentMolecule.Atoms.Count < 2) return;

            var centralAtom = CurrentMolecule.Atoms
                .OrderByDescending(a => a.BondedAtomIds.Count)
                .First();

            var bondedAtoms = CurrentMolecule.Atoms
                .Where(a => a.BondedAtomIds.Contains(centralAtom.Id))
                .ToList();

            // Posicionar átomo central na origem
            centralAtom.Position = new Point3D(0, 0, 0);

            // Posicionar átomos ligados baseado na geometria
            double bondLength = 1.5;
            
            switch (geometry)
            {
                case MoleculeGeometry.Linear:
                    for (int i = 0; i < bondedAtoms.Count; i++)
                    {
                        bondedAtoms[i].Position = new Point3D(bondLength * (i == 0 ? 1 : -1), 0, 0);
                    }
                    break;

                case MoleculeGeometry.TrigonalPlanar:
                    for (int i = 0; i < bondedAtoms.Count; i++)
                    {
                        var angle = i * 120 * Math.PI / 180;
                        bondedAtoms[i].Position = new Point3D(
                            bondLength * Math.Cos(angle),
                            bondLength * Math.Sin(angle),
                            0);
                    }
                    break;

                case MoleculeGeometry.Tetrahedral:
                    // Geometria tetraédrica
                    var tetraPositions = new[]
                    {
                        new Point3D(1, 1, 1),
                        new Point3D(-1, -1, 1),
                        new Point3D(-1, 1, -1),
                        new Point3D(1, -1, -1)
                    };
                    for (int i = 0; i < Math.Min(bondedAtoms.Count, 4); i++)
                    {
                        var pos = tetraPositions[i];
                        var length = Math.Sqrt(pos.X * pos.X + pos.Y * pos.Y + pos.Z * pos.Z);
                        bondedAtoms[i].Position = new Point3D(
                            pos.X / length * bondLength,
                            pos.Y / length * bondLength,
                            pos.Z / length * bondLength);
                    }
                    break;
            }
        }

        private void LoadTemplate(string? templateName)
        {
            if (string.IsNullOrEmpty(templateName)) return;

            ClearMolecule();

            // Templates pré-definidos
            switch (templateName.ToLower())
            {
                case "water":
                case "água":
                    BuildWaterMolecule();
                    break;
                case "methane":
                case "metano":
                    BuildMethaneMolecule();
                    break;
                case "co2":
                    BuildCO2Molecule();
                    break;
                case "ammonia":
                case "amônia":
                    BuildAmmoniaMolecule();
                    break;
            }

            UpdateMoleculeProperties();
        }

        private void BuildWaterMolecule()
        {
            MoleculeName = "Água";
            
            var oxygen = AvailableElements.FirstOrDefault(e => e.Symbol == "O");
            var hydrogen = AvailableElements.FirstOrDefault(e => e.Symbol == "H");

            if (oxygen != null && hydrogen != null)
            {
                CurrentMolecule.AddAtom(oxygen, new Point3D(0, 0, 0));
                CurrentMolecule.AddAtom(hydrogen, new Point3D(-0.96, 0, 0));
                CurrentMolecule.AddAtom(hydrogen, new Point3D(0.24, 0.93, 0));

                CurrentMolecule.AddBond(0, 1, BondType.Single);
                CurrentMolecule.AddBond(0, 2, BondType.Single);

                foreach (var atom in CurrentMolecule.Atoms)
                    CurrentAtoms.Add(atom);
                foreach (var bond in CurrentMolecule.Bonds)
                    CurrentBonds.Add(bond);
            }
        }

        private void BuildMethaneMolecule()
        {
            MoleculeName = "Metano";
            
            var carbon = AvailableElements.FirstOrDefault(e => e.Symbol == "C");
            var hydrogen = AvailableElements.FirstOrDefault(e => e.Symbol == "H");

            if (carbon != null && hydrogen != null)
            {
                CurrentMolecule.AddAtom(carbon, new Point3D(0, 0, 0));
                CurrentMolecule.AddAtom(hydrogen, new Point3D(1, 1, 1));
                CurrentMolecule.AddAtom(hydrogen, new Point3D(-1, -1, 1));
                CurrentMolecule.AddAtom(hydrogen, new Point3D(-1, 1, -1));
                CurrentMolecule.AddAtom(hydrogen, new Point3D(1, -1, -1));

                for (int i = 1; i <= 4; i++)
                    CurrentMolecule.AddBond(0, i, BondType.Single);

                foreach (var atom in CurrentMolecule.Atoms)
                    CurrentAtoms.Add(atom);
                foreach (var bond in CurrentMolecule.Bonds)
                    CurrentBonds.Add(bond);
            }
        }

        private void BuildCO2Molecule()
        {
            MoleculeName = "Dióxido de Carbono";
            
            var carbon = AvailableElements.FirstOrDefault(e => e.Symbol == "C");
            var oxygen = AvailableElements.FirstOrDefault(e => e.Symbol == "O");

            if (carbon != null && oxygen != null)
            {
                CurrentMolecule.AddAtom(carbon, new Point3D(0, 0, 0));
                CurrentMolecule.AddAtom(oxygen, new Point3D(-1.2, 0, 0));
                CurrentMolecule.AddAtom(oxygen, new Point3D(1.2, 0, 0));

                CurrentMolecule.AddBond(0, 1, BondType.Double);
                CurrentMolecule.AddBond(0, 2, BondType.Double);

                foreach (var atom in CurrentMolecule.Atoms)
                    CurrentAtoms.Add(atom);
                foreach (var bond in CurrentMolecule.Bonds)
                    CurrentBonds.Add(bond);
            }
        }

        private void BuildAmmoniaMolecule()
        {
            MoleculeName = "Amônia";
            
            var nitrogen = AvailableElements.FirstOrDefault(e => e.Symbol == "N");
            var hydrogen = AvailableElements.FirstOrDefault(e => e.Symbol == "H");

            if (nitrogen != null && hydrogen != null)
            {
                CurrentMolecule.AddAtom(nitrogen, new Point3D(0, 0, 0));
                CurrentMolecule.AddAtom(hydrogen, new Point3D(1, 0, 0));
                CurrentMolecule.AddAtom(hydrogen, new Point3D(-0.5, 0.87, 0));
                CurrentMolecule.AddAtom(hydrogen, new Point3D(-0.5, -0.87, 0));

                CurrentMolecule.AddBond(0, 1, BondType.Single);
                CurrentMolecule.AddBond(0, 2, BondType.Single);
                CurrentMolecule.AddBond(0, 3, BondType.Single);

                foreach (var atom in CurrentMolecule.Atoms)
                    CurrentAtoms.Add(atom);
                foreach (var bond in CurrentMolecule.Bonds)
                    CurrentBonds.Add(bond);
            }
        }

        private void UpdateMoleculeProperties()
        {
            CurrentMolecule.CalculateMolarMass();
            OnPropertyChanged(nameof(MolecularFormula));
            OnPropertyChanged(nameof(MolarMass));
            OnPropertyChanged(nameof(TotalCharge));
        }
    }
}
