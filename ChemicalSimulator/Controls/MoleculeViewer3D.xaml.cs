using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using HelixToolkit.Wpf;
using ChemicalSimulator.Models;
using Microsoft.Win32;

namespace ChemicalSimulator.Controls
{
    public partial class MoleculeViewer3D : UserControl
    {
        private RenderMode _currentRenderMode = RenderMode.BallAndStick;

        public MoleculeViewer3D()
        {
            InitializeComponent();
        }

        public enum RenderMode
        {
            BallAndStick,
            SpaceFilling,
            Wireframe
        }

        public void RenderMolecule(Molecule molecule)
        {
            if (molecule == null) return;

            MoleculeModel.Children.Clear();

            switch (_currentRenderMode)
            {
                case RenderMode.BallAndStick:
                    RenderBallAndStick(molecule);
                    break;
                case RenderMode.SpaceFilling:
                    RenderSpaceFilling(molecule);
                    break;
                case RenderMode.Wireframe:
                    RenderWireframe(molecule);
                    break;
            }

            Viewport.ZoomExtents(500);
        }

        private void RenderBallAndStick(Molecule molecule)
        {
            // Renderizar átomos como esferas menores
            foreach (var atom in molecule.Atoms)
            {
                var radius = GetAtomRadius(atom.Element.Symbol) * 0.3; // 30% do tamanho real
                var sphere = new SphereVisual3D
                {
                    Center = atom.Position,
                    Radius = radius,
                    Fill = new SolidColorBrush(atom.Element.DisplayColor),
                    Material = new DiffuseMaterial(new SolidColorBrush(atom.Element.DisplayColor))
                };
                MoleculeModel.Children.Add(sphere);

                // Label do elemento
                var text = new BillboardTextVisual3D
                {
                    Text = atom.Element.Symbol,
                    Position = atom.Position,
                    Foreground = Brushes.Black,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold
                };
                MoleculeModel.Children.Add(text);
            }

            // Renderizar ligações como cilindros
            foreach (var bond in molecule.Bonds)
            {
                var diameter = GetBondDiameter(bond.Type);

                // Para ligações múltiplas, renderizar cilindros paralelos
                if (bond.Type == BondType.Double)
                {
                    RenderDoubleBond(bond);
                }
                else if (bond.Type == BondType.Triple)
                {
                    RenderTripleBond(bond);
                }
                else
                {
                    var pipe = new PipeVisual3D
                    {
                        Point1 = bond.Atom1.Position,
                        Point2 = bond.Atom2.Position,
                        Diameter = diameter,
                        Fill = Brushes.Gray,
                        Material = new DiffuseMaterial(Brushes.Gray)
                    };
                    MoleculeModel.Children.Add(pipe);
                }
            }
        }

        private void RenderSpaceFilling(Molecule molecule)
        {
            // Renderizar átomos em tamanho real (van der Waals)
            foreach (var atom in molecule.Atoms)
            {
                var radius = GetAtomRadius(atom.Element.Symbol);
                var sphere = new SphereVisual3D
                {
                    Center = atom.Position,
                    Radius = radius,
                    Fill = new SolidColorBrush(atom.Element.DisplayColor),
                    Material = new DiffuseMaterial(new SolidColorBrush(atom.Element.DisplayColor))
                    {
                        AmbientColor = atom.Element.DisplayColor
                    }
                };
                MoleculeModel.Children.Add(sphere);
            }
        }

        private void RenderWireframe(Molecule molecule)
        {
            // Renderizar apenas as ligações como linhas finas
            foreach (var bond in molecule.Bonds)
            {
                var pipe = new PipeVisual3D
                {
                    Point1 = bond.Atom1.Position,
                    Point2 = bond.Atom2.Position,
                    Diameter = 0.05,
                    Fill = Brushes.DarkGray
                };
                MoleculeModel.Children.Add(pipe);
            }

            // Adicionar pequenos pontos nos átomos
            foreach (var atom in molecule.Atoms)
            {
                var sphere = new SphereVisual3D
                {
                    Center = atom.Position,
                    Radius = 0.15,
                    Fill = new SolidColorBrush(atom.Element.DisplayColor)
                };
                MoleculeModel.Children.Add(sphere);
            }
        }

        private void RenderDoubleBond(Bond bond)
        {
            var midpoint = new Point3D(
                (bond.Atom1.Position.X + bond.Atom2.Position.X) / 2,
                (bond.Atom1.Position.Y + bond.Atom2.Position.Y) / 2,
                (bond.Atom1.Position.Z + bond.Atom2.Position.Z) / 2
            );

            var direction = bond.Atom2.Position - bond.Atom1.Position;
            var perpendicular = new Vector3D(-direction.Y, direction.X, 0);
            perpendicular.Normalize();
            perpendicular *= 0.1;

            // Primeira ligação
            var pipe1 = new PipeVisual3D
            {
                Point1 = bond.Atom1.Position + perpendicular,
                Point2 = bond.Atom2.Position + perpendicular,
                Diameter = 0.08,
                Fill = Brushes.Gray
            };
            MoleculeModel.Children.Add(pipe1);

            // Segunda ligação
            var pipe2 = new PipeVisual3D
            {
                Point1 = bond.Atom1.Position - perpendicular,
                Point2 = bond.Atom2.Position - perpendicular,
                Diameter = 0.08,
                Fill = Brushes.Gray
            };
            MoleculeModel.Children.Add(pipe2);
        }

        private void RenderTripleBond(Bond bond)
        {
            var direction = bond.Atom2.Position - bond.Atom1.Position;
            var perpendicular1 = new Vector3D(-direction.Y, direction.X, 0);
            perpendicular1.Normalize();
            perpendicular1 *= 0.12;

            var perpendicular2 = Vector3D.CrossProduct(direction, perpendicular1);
            perpendicular2.Normalize();
            perpendicular2 *= 0.12;

            // Três ligações
            var pipes = new[]
            {
                new Point3D[] { bond.Atom1.Position, bond.Atom2.Position },
                new Point3D[] { bond.Atom1.Position + perpendicular1, bond.Atom2.Position + perpendicular1 },
                new Point3D[] { bond.Atom1.Position + perpendicular2, bond.Atom2.Position + perpendicular2 }
            };

            foreach (var points in pipes)
            {
                var pipe = new PipeVisual3D
                {
                    Point1 = points[0],
                    Point2 = points[1],
                    Diameter = 0.06,
                    Fill = Brushes.Gray
                };
                MoleculeModel.Children.Add(pipe);
            }
        }

        private double GetAtomRadius(string symbol)
        {
            // Raios de van der Waals aproximados (em Angstroms / 10)
            return symbol switch
            {
                "H" => 0.12,
                "C" => 0.17,
                "N" => 0.155,
                "O" => 0.152,
                "F" => 0.147,
                "P" => 0.18,
                "S" => 0.18,
                "Cl" => 0.175,
                "Br" => 0.185,
                "I" => 0.198,
                _ => 0.15
            };
        }

        private double GetBondDiameter(BondType bondType)
        {
            return bondType switch
            {
                BondType.Single => 0.1,
                BondType.Double => 0.08,
                BondType.Triple => 0.06,
                BondType.Ionic => 0.12,
                _ => 0.1
            };
        }

        private void ResetCamera_Click(object sender, RoutedEventArgs e)
        {
            Viewport.ZoomExtents(500);
        }

        private void BallAndStick_Click(object sender, RoutedEventArgs e)
        {
            _currentRenderMode = RenderMode.BallAndStick;
            // Trigger re-render se houver molécula carregada
        }

        private void SpaceFilling_Click(object sender, RoutedEventArgs e)
        {
            _currentRenderMode = RenderMode.SpaceFilling;
            // Trigger re-render se houver molécula carregada
        }

        private void CaptureImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg",
                FileName = $"Molecule_{DateTime.Now:yyyyMMdd_HHmmss}.png"
            };

            if (dialog.ShowDialog() == true)
            {
                var bitmap = new RenderTargetBitmap(
                    (int)Viewport.ActualWidth,
                    (int)Viewport.ActualHeight,
                    96, 96,
                    PixelFormats.Pbgra32);

                bitmap.Render(Viewport);

                var encoder = dialog.FileName.EndsWith(".jpg")
                    ? (BitmapEncoder)new JpegBitmapEncoder()
                    : new PngBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                using (var stream = System.IO.File.Create(dialog.FileName))
                {
                    encoder.Save(stream);
                }

                MessageBox.Show("Imagem salva com sucesso!", "Sucesso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}