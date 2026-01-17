using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using ChemicalSimulator.ViewModels;
using HelixToolkit.Wpf;

namespace ChemicalSimulator.Views
{
    /// <summary>
    /// Interaction logic for MoleculeBuilderView.xaml
    /// </summary>
    public partial class MoleculeBuilderView : UserControl
    {
        private DispatcherTimer? _rotationTimer;
        private MoleculeBuilderViewModel ViewModel => (MoleculeBuilderViewModel)DataContext;

        public MoleculeBuilderView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is MoleculeBuilderViewModel oldVm)
            {
                oldVm.CurrentAtoms.CollectionChanged -= CurrentAtoms_CollectionChanged;
                oldVm.CurrentBonds.CollectionChanged -= CurrentBonds_CollectionChanged;
            }

            if (e.NewValue is MoleculeBuilderViewModel newVm)
            {
                newVm.CurrentAtoms.CollectionChanged += CurrentAtoms_CollectionChanged;
                newVm.CurrentBonds.CollectionChanged += CurrentBonds_CollectionChanged;
                Update3DView();
            }
        }

        private void CurrentAtoms_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Update3DView();
        }

        private void CurrentBonds_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Update3DView();
        }

        private void Update3DView()
        {
            if (ViewModel == null) return;

            MoleculeModel.Children.Clear();

            // Adicionar átomos como esferas
            foreach (var atom in ViewModel.CurrentAtoms)
            {
                var sphere = new SphereVisual3D
                {
                    Center = atom.Position,
                    Radius = GetAtomicRadius(atom.Element.Symbol),
                    Fill = new SolidColorBrush(GetElementColor(atom.Element.Category))
                };
                MoleculeModel.Children.Add(sphere);

                // Label do átomo
                var billboard = new BillboardTextVisual3D
                {
                    Text = atom.Element.Symbol,
                    Position = new Point3D(
                        atom.Position.X,
                        atom.Position.Y + GetAtomicRadius(atom.Element.Symbol) + 0.3,
                        atom.Position.Z),
                    Foreground = Brushes.Black,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold
                };
                MoleculeModel.Children.Add(billboard);
            }

            // Adicionar ligações como cilindros
            foreach (var bond in ViewModel.CurrentBonds)
            {
                var point1 = bond.Atom1.Position;
                var point2 = bond.Atom2.Position;

                var pipe = new PipeVisual3D
                {
                    Point1 = point1,
                    Point2 = point2,
                    Diameter = GetBondThickness(bond.Type),
                    Fill = new SolidColorBrush(GetBondColor(bond.Type))
                };
                MoleculeModel.Children.Add(pipe);
            }

            // Zoom para ver tudo
            if (MoleculeModel.Children.Count > 0)
            {
                Viewport3D.ZoomExtents();
            }
        }

        private double GetAtomicRadius(string symbol)
        {
            // Raios atômicos aproximados (em Angstroms, escala ajustada para visualização)
            return symbol switch
            {
                "H" => 0.3,
                "C" => 0.5,
                "N" => 0.45,
                "O" => 0.4,
                "F" => 0.35,
                "P" => 0.6,
                "S" => 0.55,
                "Cl" => 0.5,
                _ => 0.5
            };
        }

        private Color GetElementColor(Models.ElementCategory category)
        {
            return category switch
            {
                Models.ElementCategory.NonMetal => Colors.LightGreen,
                Models.ElementCategory.NobleGas => Colors.Cyan,
                Models.ElementCategory.AlkaliMetal => Colors.Red,
                Models.ElementCategory.AlkalineEarthMetal => Colors.Orange,
                Models.ElementCategory.TransitionMetal => Colors.Pink,
                Models.ElementCategory.Metalloid => Colors.Khaki,
                Models.ElementCategory.Halogen => Colors.Yellow,
                _ => Colors.Gray
            };
        }

        private double GetBondThickness(Models.BondType type)
        {
            return type switch
            {
                Models.BondType.Single => 0.15,
                Models.BondType.Double => 0.2,
                Models.BondType.Triple => 0.25,
                Models.BondType.Ionic => 0.1,
                _ => 0.15
            };
        }

        private Color GetBondColor(Models.BondType type)
        {
            return type switch
            {
                Models.BondType.Single => Colors.Gray,
                Models.BondType.Double => Colors.DarkGray,
                Models.BondType.Triple => Colors.Black,
                Models.BondType.Ionic => Colors.Blue,
                _ => Colors.Gray
            };
        }

        private void ZoomExtents_Click(object sender, RoutedEventArgs e)
        {
            Viewport3D.ZoomExtents();
        }

        private void AutoRotate_Click(object sender, RoutedEventArgs e)
        {
            if (AutoRotateToggle.IsChecked == true)
            {
                StartAutoRotation();
            }
            else
            {
                StopAutoRotation();
            }
        }

        private void StartAutoRotation()
        {
            _rotationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(30)
            };
            _rotationTimer.Tick += (s, e) =>
            {
                var camera = Viewport3D.Camera as PerspectiveCamera;
                if (camera != null)
                {
                    var transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 1));
                    camera.Position = transform.Transform(camera.Position);
                    camera.LookDirection = new Point3D(0, 0, 0) - camera.Position;
                }
            };
            _rotationTimer.Start();
        }

        private void StopAutoRotation()
        {
            _rotationTimer?.Stop();
            _rotationTimer = null;
        }
    }
}
