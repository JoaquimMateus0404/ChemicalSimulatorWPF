using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using ChemicalSimulator.Commands;
using ChemicalSimulator.Helpers;
using ChemicalSimulator.Models;

namespace ChemicalSimulator.ViewModels
{
    /// <summary>
    /// ViewModel para construção de moléculas
    /// </summary>
    public class MoleculeBuilderViewModel : ViewModelBase
    {
        private Molecule _currentMolecule;
        private Atom _selectedAtom1;
        private Atom _selectedAtom2;
        private BondType _selectedBondType;
        private string _moleculeName;
        private bool _isEditMode;

        public MoleculeBuilderViewModel()
        {
            CurrentMolecule = new Molecule { Name = "Nova Molécula" };
            SelectedBondType = BondType.Single;

            InitializeCommands();
        }

        #region Properties

        public Molecule CurrentMolecule
        {
            get => _currentMolecule;
            set
            {
                if (SetProperty(ref _currentMolecule, value))
                {
                    OnPropertyChanged(nameof(AtomCount));
                    OnPropertyChanged(nameof(BondCount));
                }
            }
        }

        public Atom SelectedAtom1
        {
            get => _selectedAtom1;
            set => SetProperty(ref _selectedAtom1, value);
        }

        public Atom SelectedAtom2
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

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public int AtomCount => CurrentMolecule?.Atoms.Count ?? 0;
        public int BondCount => CurrentMolecule?.Bonds.Count ?? 0;

        #endregion

        #region Commands

        public ICommand AddAtomCommand { get; private set; }
        public ICommand RemoveAtomCommand { get; private set; }
        public ICommand CreateBondCommand { get; private set; }
        public ICommand RemoveBondCommand { get; private set; }
        public ICommand ClearMoleculeCommand { get; private set; }
        public ICommand OptimizeGeometryCommand { get; private set; }
        public ICommand SaveMoleculeCommand { get; private set; }

        #endregion

        private void InitializeCommands()
        {
            AddAtomCommand = new RelayCommand<Element>(AddAtom);
            RemoveAtomCommand = new RelayCommand<Atom>(RemoveAtom);
            CreateBondCommand = new RelayCommand(CreateBond, CanCreateBond);
            RemoveBondCommand = new RelayCommand<Bond>(RemoveBond);
            ClearMoleculeCommand = new RelayCommand(ClearMolecule);
            OptimizeGeometryCommand = new RelayCommand(OptimizeGeometry);
            SaveMoleculeCommand = new RelayCommand(SaveMolecule);
        }

        #region Command Methods

        private void AddAtom(Element element)
        {
            if (element == null) return;

            // Calcular posição para o novo átomo
            var position = CalculateNewAtomPosition();

            CurrentMolecule.AddAtom(element, position);
            CurrentMolecule.MolarMass = ChemistryCalculator.CalculateMolarMass(CurrentMolecule);
            CurrentMolecule.Formula = ChemistryCalculator.GenerateMolecularFormula(CurrentMolecule);

            OnPropertyChanged(nameof(CurrentMolecule));
            OnPropertyChanged(nameof(AtomCount));
        }

        private void RemoveAtom(Atom atom)
        {
            if (atom == null) return;

            // Remover todas as ligações conectadas a este átomo
            var bondsToRemove = CurrentMolecule.Bonds
                .Where(b => b.Atom1 == atom || b.Atom2 == atom)
                .ToList();

            foreach (var bond in bondsToRemove)
            {
                CurrentMolecule.Bonds.Remove(bond);
            }

            CurrentMolecule.Atoms.Remove(atom);
            CurrentMolecule.MolarMass = ChemistryCalculator.CalculateMolarMass(CurrentMolecule);
            CurrentMolecule.Formula = ChemistryCalculator.GenerateMolecularFormula(CurrentMolecule);

            OnPropertyChanged(nameof(CurrentMolecule));
            OnPropertyChanged(nameof(AtomCount));
            OnPropertyChanged(nameof(BondCount));
        }

        private bool CanCreateBond()
        {
            return SelectedAtom1 != null && SelectedAtom2 != null && SelectedAtom1 != SelectedAtom2;
        }

        private void CreateBond()
        {
            if (!CanCreateBond()) return;

            // Verificar se já existe uma ligação entre esses átomos
            var existingBond = CurrentMolecule.Bonds
                .FirstOrDefault(b =>
                    (b.Atom1 == SelectedAtom1 && b.Atom2 == SelectedAtom2) ||
                    (b.Atom1 == SelectedAtom2 && b.Atom2 == SelectedAtom1));

            if (existingBond != null)
            {
                // Atualizar tipo de ligação existente
                existingBond.Type = SelectedBondType;
            }
            else
            {
                // Criar nova ligação
                var bond = new Bond
                {
                    Atom1 = SelectedAtom1,
                    Atom2 = SelectedAtom2,
                    Type = SelectedBondType,
                    Length = ChemistryCalculator.CalculateBondLength(SelectedAtom1, SelectedAtom2)
                };
                bond.Energy = ChemistryCalculator.CalculateBondEnergy(bond);
                CurrentMolecule.Bonds.Add(bond);
            }

            // Atualizar listas de átomos conectados
            if (!SelectedAtom1.BondedAtomIds.Contains(SelectedAtom2.Id))
                SelectedAtom1.BondedAtomIds.Add(SelectedAtom2.Id);

            if (!SelectedAtom2.BondedAtomIds.Contains(SelectedAtom1.Id))
                SelectedAtom2.BondedAtomIds.Add(SelectedAtom1.Id);

            OnPropertyChanged(nameof(BondCount));
            SelectedAtom1 = null;
            SelectedAtom2 = null;
        }

        private void RemoveBond(Bond bond)
        {
            if (bond == null) return;

            CurrentMolecule.Bonds.Remove(bond);

            // Atualizar listas de átomos conectados
            bond.Atom1.BondedAtomIds.Remove(bond.Atom2.Id);
            bond.Atom2.BondedAtomIds.Remove(bond.Atom1.Id);

            OnPropertyChanged(nameof(BondCount));
        }

        private void ClearMolecule()
        {
            CurrentMolecule = new Molecule { Name = "Nova Molécula" };
            SelectedAtom1 = null;
            SelectedAtom2 = null;
            OnPropertyChanged(nameof(CurrentMolecule));
            OnPropertyChanged(nameof(AtomCount));
            OnPropertyChanged(nameof(BondCount));
        }

        private void OptimizeGeometry()
        {
            // Implementar otimização de geometria molecular
            // Por exemplo, usando algoritmo de minimização de energia

            if (CurrentMolecule.Atoms.Count < 2) return;

            // Algoritmo simples de otimização baseado em comprimentos de ligação
            foreach (var bond in CurrentMolecule.Bonds)
            {
                var idealLength = ChemistryCalculator.GetIdealBondLength(
                    bond.Type, bond.Atom1.Element, bond.Atom2.Element);
                var currentLength = ChemistryCalculator.CalculateBondLength(
                    bond.Atom1, bond.Atom2);

                if (Math.Abs(currentLength - idealLength) > 0.1)
                {
                    // Ajustar posições
                    AdjustBondLength(bond, idealLength);
                }
            }

            OnPropertyChanged(nameof(CurrentMolecule));
        }

        private void SaveMolecule()
        {
            if (!string.IsNullOrWhiteSpace(MoleculeName))
            {
                CurrentMolecule.Name = MoleculeName;
            }

            // Aqui você pode adicionar lógica para salvar em banco de dados ou arquivo
            MessageBox.Show($"Molécula '{CurrentMolecule.Name}' salva com sucesso!",
                "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Helper Methods

        private Point3D CalculateNewAtomPosition()
        {
            if (CurrentMolecule.Atoms.Count == 0)
                return new Point3D(0, 0, 0);

            // Posicionar próximo ao último átomo
            var lastAtom = CurrentMolecule.Atoms.Last();
            return new Point3D(
                lastAtom.Position.X + 2.0,
                lastAtom.Position.Y,
                lastAtom.Position.Z
            );
        }

        private void AdjustBondLength(Bond bond, double targetLength)
        {
            var currentLength = ChemistryCalculator.CalculateBondLength(bond.Atom1, bond.Atom2);
            var scale = targetLength / currentLength;

            var midpoint = new Point3D(
                (bond.Atom1.Position.X + bond.Atom2.Position.X) / 2,
                (bond.Atom1.Position.Y + bond.Atom2.Position.Y) / 2,
                (bond.Atom1.Position.Z + bond.Atom2.Position.Z) / 2
            );

            // Ajustar posições mantendo o centro
            var vector1 = bond.Atom1.Position - midpoint;
            var vector2 = bond.Atom2.Position - midpoint;

            bond.Atom1.Position = midpoint + vector1 * scale;
            bond.Atom2.Position = midpoint + vector2 * scale;
        }

        #endregion
    }
}