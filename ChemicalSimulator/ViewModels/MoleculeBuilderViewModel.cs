using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using ChemicalSimulator.Models;
using ChemicalSimulator.Services;

namespace ChemicalSimulator.ViewModels
{
    /// <summary>
    /// ViewModel para construção de moléculas
    /// </summary>
    public class MoleculeBuilderViewModel : INotifyPropertyChanged
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
                _currentMolecule = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AtomCount));
                OnPropertyChanged(nameof(BondCount));
            }
        }

        public Atom SelectedAtom1
        {
            get => _selectedAtom1;
            set
            {
                _selectedAtom1 = value;
                OnPropertyChanged();
            }
        }

        public Atom SelectedAtom2
        {
            get => _selectedAtom2;
            set
            {
                _selectedAtom2 = value;
                OnPropertyChanged();
            }
        }

        public BondType SelectedBondType
        {
            get => _selectedBondType;
            set
            {
                _selectedBondType = value;
                OnPropertyChanged();
            }
        }

        public string MoleculeName
        {
            get => _moleculeName;
            set
            {
                _moleculeName = value;
                OnPropertyChanged();
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                _isEditMode = value;
                OnPropertyChanged();
            }
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
            CurrentMolecule.CalculateMolarMass();
            CurrentMolecule.Formula = CurrentMolecule.GetMolecularFormula();

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
            CurrentMolecule.CalculateMolarMass();
            CurrentMolecule.Formula = CurrentMolecule.GetMolecularFormula();

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
                    Length = CalculateBondLength(SelectedAtom1.Position, SelectedAtom2.Position)
                };
                bond.Energy = bond.GetBondEnergy();
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
                var idealLength = GetIdealBondLength(bond.Type, bond.Atom1.Element, bond.Atom2.Element);
                var currentLength = CalculateBondLength(bond.Atom1.Position, bond.Atom2.Position);

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

        private double CalculateBondLength(Point3D p1, Point3D p2)
        {
            var dx = p1.X - p2.X;
            var dy = p1.Y - p2.Y;
            var dz = p1.Z - p2.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        private double GetIdealBondLength(BondType bondType, Element e1, Element e2)
        {
            // Comprimentos de ligação ideais em Angstroms
            var baseLengths = new System.Collections.Generic.Dictionary<string, double>
            {
                { "C-C", 1.54 }, { "C=C", 1.34 }, { "C≡C", 1.20 },
                { "C-H", 1.09 }, { "O-H", 0.96 }, { "N-H", 1.01 },
                { "C-O", 1.43 }, { "C=O", 1.20 }, { "C-N", 1.47 },
                { "N=N", 1.25 }, { "N≡N", 1.10 }, { "O=O", 1.21 }
            };

            var key = bondType switch
            {
                BondType.Double => $"{e1.Symbol}={e2.Symbol}",
                BondType.Triple => $"{e1.Symbol}≡{e2.Symbol}",
                _ => $"{e1.Symbol}-{e2.Symbol}"
            };

            return baseLengths.ContainsKey(key) ? baseLengths[key] : 1.5;
        }

        private void AdjustBondLength(Bond bond, double targetLength)
        {
            var currentLength = CalculateBondLength(bond.Atom1.Position, bond.Atom2.Position);
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

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}