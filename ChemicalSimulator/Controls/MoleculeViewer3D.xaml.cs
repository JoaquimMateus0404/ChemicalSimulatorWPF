using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using ChemicalSimulator.Models;

namespace ChemicalSimulator.Controls
{
    public partial class MoleculeViewer3D : UserControl
    {
        public MoleculeViewer3D()
        {
            InitializeComponent();
        }

        public void RenderMolecule(Molecule molecule)
        {
            MoleculeModel.Children.Clear();

            // Renderizar átomos
            foreach (var atom in molecule.Atoms)
            {
                var sphere = new SphereVisual3D
                {
                    Center = atom.Position,
                    Radius = atom.Element.VanDerWaalsRadius / 100.0,
                    Fill = new SolidColorBrush(atom.Element.DisplayColor)
                };
                MoleculeModel.Children.Add(sphere);

                // Adicionar label do elemento
                var text = new BillboardTextVisual3D
                {
                    Text = atom.Element.Symbol,
                    Position = atom.Position,
                    Foreground = Brushes.Black,
                    FontSize = 12
                };
                MoleculeModel.Children.Add(text);
            }

            // Renderizar ligações
            foreach (var bond in molecule.Bonds)
            {
                var pipe = new PipeVisual3D
                {
                    Point1 = bond.Atom1.Position,
                    Point2 = bond.Atom2.Position,
                    Diameter = bond.Type == BondType.Single ? 0.1 :
                               bond.Type == BondType.Double ? 0.15 : 0.2,
                    Fill = Brushes.Gray
                };
                MoleculeModel.Children.Add(pipe);
            }

            // Ajustar câmera
            Viewport.ZoomExtents();
        }
    }
}