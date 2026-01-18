using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using ChemicalSimulator.ViewModels;
using ChemicalSimulator.Models;
using HelixToolkit.Wpf;

namespace ChemicalSimulator.Views
{
    /// <summary>
    /// Code-behind para MoleculeBuilderView com renderização 2D e 3D
    /// </summary>
    public partial class MoleculeBuilderView : UserControl
    {
        private MoleculeBuilderViewModel? ViewModel => DataContext as MoleculeBuilderViewModel;

        public MoleculeBuilderView()
        {
            InitializeComponent();
            
            // Subscrever mudanças na coleção de átomos e ligações
            Loaded += MoleculeBuilderView_Loaded;
        }

        private void MoleculeBuilderView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.Atoms.CollectionChanged += (s, args) => Render2DMolecule();
                ViewModel.Bonds.CollectionChanged += (s, args) => Render2DMolecule();
                
                // Renderizar 3D também
                ViewModel.Atoms.CollectionChanged += (s, args) => Render3DMolecule();
                ViewModel.Bonds.CollectionChanged += (s, args) => Render3DMolecule();
                
                // Monitorar mudanças na propriedade Show3DView para centralizar molécula
                ViewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(ViewModel.Show3DView) && ViewModel.Show3DView)
                    {
                        CenterAndFitMolecule();
                    }
                };
            }
        }

        /// <summary>
        /// Event handler para mudança de tipo de ligação
        /// </summary>
        private void BondType_Checked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;
            
            var radioButton = sender as RadioButton;
            if (radioButton?.Tag is string bondTypeStr)
            {
                if (Enum.TryParse<BondType>(bondTypeStr, out var bondType))
                {
                    ViewModel.SelectedBondType = bondType;
                }
            }
        }

        /// <summary>
        /// Evento de clique no canvas 2D para adicionar átomos
        /// </summary>
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel == null || ViewModel.SelectedElement == null) return;

            Point clickPosition = e.GetPosition(sender as IInputElement);
            
            // Verificar se clicou em um átomo existente (para criar ligação)
            var clickedAtom = FindAtomAtPosition(clickPosition);
            
            if (clickedAtom != null)
            {
                // Criar ligação
                if (ViewModel.AddBondCommand.CanExecute(clickedAtom))
                {
                    ViewModel.AddBondCommand.Execute(clickedAtom);
                }
            }
            else
            {
                // Adicionar novo átomo
                if (ViewModel.AddAtomCommand.CanExecute(clickPosition))
                {
                    ViewModel.AddAtomCommand.Execute(clickPosition);
                }
            }
        }

        /// <summary>
        /// Encontra um átomo próximo à posição clicada
        /// </summary>
        private Atom? FindAtomAtPosition(Point position)
        {
            if (ViewModel?.Atoms == null) return null;

            const double clickRadius = 20; // Raio de detecção em pixels

            return ViewModel.Atoms.FirstOrDefault(atom =>
            {
                double dx = atom.Position.X - position.X;
                double dy = atom.Position.Y - position.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);
                return distance <= clickRadius;
            });
        }

        /// <summary>
        /// Renderiza a molécula no canvas 2D
        /// </summary>
        private void Render2DMolecule()
        {
            if (MoleculeCanvas == null) return;
            
            MoleculeCanvas.Children.Clear();

            if (ViewModel == null) return;

            // Renderizar ligações primeiro (para ficarem atrás dos átomos)
            foreach (var bond in ViewModel.Bonds)
            {
                DrawBond2D(bond);
            }

            // Renderizar átomos
            foreach (var atom in ViewModel.Atoms)
            {
                DrawAtom2D(atom);
            }

            // Renderizar visualização 3D
            Render3DMolecule();
        }

        /// <summary>
        /// Desenha um átomo no canvas 2D
        /// </summary>
        private void DrawAtom2D(Atom atom)
        {
            if (MoleculeCanvas == null) return;
            
            const double atomRadius = 20;

            // Círculo do átomo
            var ellipse = new Ellipse
            {
                Width = atomRadius * 2,
                Height = atomRadius * 2,
                Fill = new SolidColorBrush(GetElementColor(atom.Element)),
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Tag = atom
            };

            // Posicionar
            Canvas.SetLeft(ellipse, atom.Position.X - atomRadius);
            Canvas.SetTop(ellipse, atom.Position.Y - atomRadius);

            // Adicionar interatividade
            ellipse.MouseEnter += (s, e) =>
            {
                ellipse.StrokeThickness = 3;
                ellipse.Stroke = Brushes.Blue;
            };

            ellipse.MouseLeave += (s, e) =>
            {
                ellipse.StrokeThickness = 2;
                ellipse.Stroke = Brushes.Black;
            };

            MoleculeCanvas.Children.Add(ellipse);

            // Símbolo do elemento
            var textBlock = new TextBlock
            {
                Text = atom.Element.Symbol,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Canvas.SetLeft(textBlock, atom.Position.X - 10);
            Canvas.SetTop(textBlock, atom.Position.Y - 10);

            MoleculeCanvas.Children.Add(textBlock);
        }

        /// <summary>
        /// Desenha uma ligação no canvas 2D
        /// </summary>
        private void DrawBond2D(Bond bond)
        {
            var p1 = bond.Atom1.Position;
            var p2 = bond.Atom2.Position;

            switch (bond.Type)
            {
                case BondType.Single:
                    DrawSingleBond(p1, p2);
                    break;
                case BondType.Double:
                    DrawDoubleBond(p1, p2);
                    break;
                case BondType.Triple:
                    DrawTripleBond(p1, p2);
                    break;
                default:
                    DrawSingleBond(p1, p2);
                    break;
            }
        }

        private void DrawSingleBond(Point3D p1, Point3D p2)
        {
            if (MoleculeCanvas == null) return;
            
            var line = new Line
            {
                X1 = p1.X,
                Y1 = p1.Y,
                X2 = p2.X,
                Y2 = p2.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            MoleculeCanvas.Children.Add(line);
        }

        private void DrawDoubleBond(Point3D p1, Point3D p2)
        {
            if (MoleculeCanvas == null) return;
            
            // Calcular vetor perpendicular
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);
            double perpX = -dy / length * 3;
            double perpY = dx / length * 3;

            // Primeira linha
            var line1 = new Line
            {
                X1 = p1.X + perpX,
                Y1 = p1.Y + perpY,
                X2 = p2.X + perpX,
                Y2 = p2.Y + perpY,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            MoleculeCanvas.Children.Add(line1);

            // Segunda linha
            var line2 = new Line
            {
                X1 = p1.X - perpX,
                Y1 = p1.Y - perpY,
                X2 = p2.X - perpX,
                Y2 = p2.Y - perpY,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            MoleculeCanvas.Children.Add(line2);
        }

        private void DrawTripleBond(Point3D p1, Point3D p2)
        {
            if (MoleculeCanvas == null) return;
            
            // Linha central
            DrawSingleBond(p1, p2);

            // Calcular vetor perpendicular
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);
            double perpX = -dy / length * 5;
            double perpY = dx / length * 5;

            // Linha superior
            var line1 = new Line
            {
                X1 = p1.X + perpX,
                Y1 = p1.Y + perpY,
                X2 = p2.X + perpX,
                Y2 = p2.Y + perpY,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            MoleculeCanvas.Children.Add(line1);

            // Linha inferior
            var line2 = new Line
            {
                X1 = p1.X - perpX,
                Y1 = p1.Y - perpY,
                X2 = p2.X - perpX,
                Y2 = p2.Y - perpY,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            MoleculeCanvas.Children.Add(line2);
        }

        /// <summary>
        /// Renderiza a molécula em 3D usando Helix Toolkit
        /// </summary>
        private void Render3DMolecule()
        {
            if (ViewModel == null || Viewport3D == null) return;

            // Limpar viewport (manter apenas as luzes)
            var children = Viewport3D.Children.OfType<ModelVisual3D>().ToList();
            foreach (var child in children)
            {
                if (!(child is DefaultLights))
                {
                    Viewport3D.Children.Remove(child);
                }
            }

            // Renderizar átomos em 3D
            foreach (var atom in ViewModel.Atoms)
            {
                DrawAtom3D(atom);
            }

            // Renderizar ligações em 3D
            foreach (var bond in ViewModel.Bonds)
            {
                DrawBond3D(bond);
            }
        }

        private void DrawAtom3D(Atom atom)
        {
            if (Viewport3D == null || ViewModel == null) return;
            
            var position = new Point3D(atom.Position.X / 50, atom.Position.Y / 50, atom.Position.Z / 50);
            double radius = ViewModel.AtomSize3D;

            // Ajustar raio baseado no estilo
            if (ViewModel.ViewStyle3D == "SpaceFilling")
            {
                radius *= 2.0; // Átomos maiores no modo space-filling
            }
            else if (ViewModel.ViewStyle3D == "Wireframe")
            {
                radius *= 0.3; // Átomos menores no modo wireframe
            }

            var sphere = new SphereVisual3D
            {
                Center = position,
                Radius = radius,
                Fill = new SolidColorBrush(GetElementColor(atom.Element))
            };

            Viewport3D.Children.Add(sphere);

            // Adicionar etiqueta se habilitado
            if (ViewModel.ShowLabels3D)
            {
                var label = new BillboardTextVisual3D
                {
                    Text = atom.Element.Symbol,
                    Position = position,
                    Foreground = Brushes.Black,
                    FontSize = 12,
                    FontWeight = FontWeights.Bold
                };
                Viewport3D.Children.Add(label);
            }
        }

        private void DrawBond3D(Bond bond)
        {
            if (Viewport3D == null || ViewModel == null) return;
            
            var p1 = new Point3D(bond.Atom1.Position.X / 50, bond.Atom1.Position.Y / 50, bond.Atom1.Position.Z / 50);
            var p2 = new Point3D(bond.Atom2.Position.X / 50, bond.Atom2.Position.Y / 50, bond.Atom2.Position.Z / 50);

            double thickness = ViewModel.BondThickness3D;

            // Modo wireframe usa linhas mais finas
            if (ViewModel.ViewStyle3D == "Wireframe")
            {
                thickness *= 0.5;
            }

            var pipe = new PipeVisual3D
            {
                Point1 = p1,
                Point2 = p2,
                Diameter = thickness,
                Fill = Brushes.DarkGray
            };

            Viewport3D.Children.Add(pipe);

            // Desenhar ligações múltiplas
            if (bond.Type == BondType.Double)
            {
                // Calcular deslocamento perpendicular
                var direction = p2 - p1;
                var perpendicular = Vector3D.CrossProduct(direction, new Vector3D(0, 1, 0));
                perpendicular.Normalize();
                perpendicular *= thickness * 2;

                var pipe2 = new PipeVisual3D
                {
                    Point1 = p1 + perpendicular,
                    Point2 = p2 + perpendicular,
                    Diameter = thickness,
                    Fill = Brushes.DarkGray
                };
                Viewport3D.Children.Add(pipe2);
            }
            else if (bond.Type == BondType.Triple)
            {
                // Calcular deslocamentos perpendiculares
                var direction = p2 - p1;
                var perp1 = Vector3D.CrossProduct(direction, new Vector3D(0, 1, 0));
                perp1.Normalize();
                perp1 *= thickness * 2;

                var perp2 = Vector3D.CrossProduct(direction, new Vector3D(1, 0, 0));
                perp2.Normalize();
                perp2 *= thickness * 2;

                var pipe2 = new PipeVisual3D
                {
                    Point1 = p1 + perp1,
                    Point2 = p2 + perp1,
                    Diameter = thickness,
                    Fill = Brushes.DarkGray
                };
                Viewport3D.Children.Add(pipe2);

                var pipe3 = new PipeVisual3D
                {
                    Point1 = p1 + perp2,
                    Point2 = p2 + perp2,
                    Diameter = thickness,
                    Fill = Brushes.DarkGray
                };
                Viewport3D.Children.Add(pipe3);
            }
        }

        /// <summary>
        /// Retorna a cor característica de um elemento
        /// </summary>
        private Color GetElementColor(Element element)
        {
            return element.Symbol switch
            {
                "H" => Colors.White,
                "C" => Colors.Black,
                "N" => Colors.Blue,
                "O" => Colors.Red,
                "F" => Colors.Green,
                "Cl" => Colors.LightGreen,
                "Br" => Colors.Brown,
                "I" => Colors.Purple,
                "P" => Colors.Orange,
                "S" => Colors.Yellow,
                _ => Colors.Gray
            };
        }

        #region Funcionalidades 3D Avançadas

        private System.Windows.Threading.DispatcherTimer? _rotationTimer;
        private bool _isAutoRotating = false;
        private List<Atom> _selectedAtomsForMeasurement = new List<Atom>();
        private string _measurementMode = ""; // "distance" ou "angle"

        /// <summary>
        /// Alterna rotação automática da molécula
        /// </summary>
        private void ToggleAutoRotation(object sender, RoutedEventArgs e)
        {
            if (_isAutoRotating)
            {
                _rotationTimer?.Stop();
                _isAutoRotating = false;
                ((Button)sender).Content = "🔄 Auto Rotação";
            }
            else
            {
                if (_rotationTimer == null)
                {
                    _rotationTimer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(50)
                    };
                    _rotationTimer.Tick += (s, args) =>
                    {
                        if (Viewport3D?.Camera is PerspectiveCamera camera)
                        {
                            var axis = new Vector3D(0, 1, 0);
                            var rotation = new AxisAngleRotation3D(axis, 1);
                            var transform = new RotateTransform3D(rotation);
                            camera.Position = transform.Transform(camera.Position);
                            camera.LookDirection = new Point3D(0, 0, 0) - camera.Position;
                        }
                    };
                }
                _rotationTimer.Start();
                _isAutoRotating = true;
                ((Button)sender).Content = "⏸️ Pausar Rotação";
            }
        }

        /// <summary>
        /// Inicia modo de medição de distância
        /// </summary>
        private void MeasureDistance(object sender, RoutedEventArgs e)
        {
            _measurementMode = "distance";
            _selectedAtomsForMeasurement.Clear();
            MeasurementPanel.Visibility = Visibility.Visible;
            MeasurementTitle.Text = "📏 Medição de Distância";
            MeasurementValue.Text = "Selecione 2 átomos...";
        }

        /// <summary>
        /// Inicia modo de medição de ângulo
        /// </summary>
        private void MeasureAngle(object sender, RoutedEventArgs e)
        {
            _measurementMode = "angle";
            _selectedAtomsForMeasurement.Clear();
            MeasurementPanel.Visibility = Visibility.Visible;
            MeasurementTitle.Text = "📐 Medição de Ângulo";
            MeasurementValue.Text = "Selecione 3 átomos...";
        }

        /// <summary>
        /// Reset da câmera para posição padrão
        /// </summary>
        private void ResetCamera(object sender, RoutedEventArgs e)
        {
            CenterAndFitMolecule();
        }

        /// <summary>
        /// Centraliza e ajusta a câmera para visualizar toda a molécula
        /// </summary>
        private void CenterAndFitMolecule()
        {
            if (Viewport3D == null) return;

            // Se houver átomos, calcular o centro da molécula
            if (ViewModel?.Atoms != null && ViewModel.Atoms.Any())
            {
                // Calcular o centro geométrico da molécula
                double centerX = ViewModel.Atoms.Average(a => a.Position.X) / 50;
                double centerY = ViewModel.Atoms.Average(a => a.Position.Y) / 50;
                double centerZ = ViewModel.Atoms.Average(a => a.Position.Z) / 50;

                // Calcular o tamanho da molécula para ajustar a distância da câmera
                double maxX = ViewModel.Atoms.Max(a => a.Position.X) / 50;
                double minX = ViewModel.Atoms.Min(a => a.Position.X) / 50;
                double maxY = ViewModel.Atoms.Max(a => a.Position.Y) / 50;
                double minY = ViewModel.Atoms.Min(a => a.Position.Y) / 50;
                double maxZ = ViewModel.Atoms.Max(a => a.Position.Z) / 50;
                double minZ = ViewModel.Atoms.Min(a => a.Position.Z) / 50;

                double sizeX = maxX - minX;
                double sizeY = maxY - minY;
                double sizeZ = maxZ - minZ;
                double moleculeSize = Math.Max(Math.Max(sizeX, sizeY), sizeZ);

                // Distância da câmera baseada no tamanho da molécula
                double cameraDistance = Math.Max(10, moleculeSize * 3);

                if (Viewport3D.Camera is PerspectiveCamera camera)
                {
                    // Posicionar câmera olhando para o centro da molécula
                    camera.Position = new Point3D(
                        centerX + cameraDistance,
                        centerY + cameraDistance,
                        centerZ + cameraDistance
                    );
                    camera.LookDirection = new Vector3D(
                        -cameraDistance,
                        -cameraDistance,
                        -cameraDistance
                    );
                    camera.UpDirection = new Vector3D(0, 1, 0);
                    camera.FieldOfView = 45;
                }
            }
            else
            {
                // Posição padrão se não houver átomos
                if (Viewport3D.Camera is PerspectiveCamera camera)
                {
                    camera.Position = new Point3D(10, 10, 10);
                    camera.LookDirection = new Vector3D(-10, -10, -10);
                    camera.UpDirection = new Vector3D(0, 1, 0);
                    camera.FieldOfView = 45;
                }
            }

            // Usar o método ZoomExtents do Helix Toolkit para ajustar automaticamente
            Viewport3D.ZoomExtents(500); // 500ms de animação
        }

        /// <summary>
        /// Centraliza a molécula na vista 3D
        /// </summary>
        private void CenterMolecule(object sender, RoutedEventArgs e)
        {
            CenterAndFitMolecule();
        }

        /// <summary>
        /// Clique em átomo no viewport 3D
        /// </summary>
        private void Viewport3D_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel == null || string.IsNullOrEmpty(_measurementMode)) return;

            var viewport = sender as HelixViewport3D;
            if (viewport == null) return;

            var position = e.GetPosition(viewport);
            var hitResult = HelixToolkit.Wpf.Viewport3DHelper.FindHits(viewport.Viewport, position);

            if (hitResult != null && hitResult.Any())
            {
                // Encontrar átomo mais próximo do clique
                var hit = hitResult.First();
                var atom = FindNearestAtom(hit.Position);

                if (atom != null)
                {
                    _selectedAtomsForMeasurement.Add(atom);

                    if (_measurementMode == "distance" && _selectedAtomsForMeasurement.Count == 2)
                    {
                        CalculateDistance();
                    }
                    else if (_measurementMode == "angle" && _selectedAtomsForMeasurement.Count == 3)
                    {
                        CalculateAngle();
                    }
                    else
                    {
                        int needed = _measurementMode == "distance" ? 2 : 3;
                        int selected = _selectedAtomsForMeasurement.Count;
                        MeasurementValue.Text = $"{selected}/{needed} átomos selecionados";
                    }
                }
            }
        }

        private Atom? FindNearestAtom(Point3D clickPosition)
        {
            if (ViewModel == null) return null;

            Atom? nearest = null;
            double minDistance = double.MaxValue;

            foreach (var atom in ViewModel.Atoms)
            {
                var atomPos = new Point3D(atom.Position.X / 50, atom.Position.Y / 50, atom.Position.Z / 50);
                var distance = (clickPosition - atomPos).Length;

                if (distance < minDistance && distance < 1.0) // Tolerância de 1 unidade
                {
                    minDistance = distance;
                    nearest = atom;
                }
            }

            return nearest;
        }

        private void CalculateDistance()
        {
            if (_selectedAtomsForMeasurement.Count != 2) return;

            var atom1 = _selectedAtomsForMeasurement[0];
            var atom2 = _selectedAtomsForMeasurement[1];

            var p1 = atom1.Position;
            var p2 = atom2.Position;

            var dx = p2.X - p1.X;
            var dy = p2.Y - p1.Y;
            var dz = p2.Z - p1.Z;
            var distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);

            // Converter de pixels para Angstroms (aproximado)
            var distanceAngstroms = distance / 40.0;

            MeasurementValue.Text = $"{atom1.Element.Symbol}-{atom2.Element.Symbol}: {distanceAngstroms:F2} Å";

            // Resetar após 5 segundos
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            timer.Tick += (s, e) =>
            {
                _selectedAtomsForMeasurement.Clear();
                _measurementMode = "";
                MeasurementPanel.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }

        private void CalculateAngle()
        {
            if (_selectedAtomsForMeasurement.Count != 3) return;

            var atom1 = _selectedAtomsForMeasurement[0];
            var atom2 = _selectedAtomsForMeasurement[1]; // Vértice
            var atom3 = _selectedAtomsForMeasurement[2];

            var v1 = atom1.Position - atom2.Position;
            var v2 = atom3.Position - atom2.Position;

            var dotProduct = v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
            var length1 = Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y + v1.Z * v1.Z);
            var length2 = Math.Sqrt(v2.X * v2.X + v2.Y * v2.Y + v2.Z * v2.Z);

            var cosAngle = dotProduct / (length1 * length2);
            var angleRadians = Math.Acos(Math.Clamp(cosAngle, -1.0, 1.0));
            var angleDegrees = angleRadians * 180.0 / Math.PI;

            MeasurementValue.Text = $"{atom1.Element.Symbol}-{atom2.Element.Symbol}-{atom3.Element.Symbol}: {angleDegrees:F1}°";

            // Resetar após 5 segundos
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            timer.Tick += (s, e) =>
            {
                _selectedAtomsForMeasurement.Clear();
                _measurementMode = "";
                MeasurementPanel.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }

        /// <summary>
        /// Mudança no estilo de visualização 3D
        /// </summary>
        private void ViewStyle_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null || ViewStyleCombo == null) return;

            var selected = ViewStyleCombo.SelectedItem as ComboBoxItem;
            if (selected?.Tag is string style)
            {
                ViewModel.ViewStyle3D = style;
                Render3DMolecule();
            }
        }

        /// <summary>
        /// Mudança no tamanho dos átomos
        /// </summary>
        private void AtomSize_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ViewModel == null) return;
            ViewModel.AtomSize3D = e.NewValue;
            Render3DMolecule();
        }

        /// <summary>
        /// Mudança na espessura das ligações
        /// </summary>
        private void BondThickness_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ViewModel == null) return;
            ViewModel.BondThickness3D = e.NewValue;
            Render3DMolecule();
        }

        /// <summary>
        /// Toggle de etiquetas dos átomos
        /// </summary>
        private void ShowLabels_Changed(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;
            ViewModel.ShowLabels3D = ShowLabelsCheckbox.IsChecked ?? false;
            Render3DMolecule();
        }

        #endregion
    }
}
