using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using ChemicalSimulator.Models;

namespace ChemicalSimulator.Controls
{
    /// <summary>
    /// UserControl para animação molecular 3D DINÂMICA usando Helix Toolkit
    /// Mostra reagentes e produtos REAIS da reação com cores dos elementos
    /// </summary>
    public partial class MolecularAnimation3DControl : UserControl
    {
        private Storyboard? _currentStoryboard;
        private List<Visual3D> _reagentVisuals = new();
        private List<Visual3D> _productVisuals = new();
        
        // Cores específicas para cada elemento químico (CPK colors)
        private static readonly Dictionary<string, Color> ElementColors = new()
        {
            { "H", Color.FromRgb(255, 255, 255) },  // Branco
            { "C", Color.FromRgb(144, 144, 144) },  // Cinza escuro
            { "N", Color.FromRgb(48, 80, 248) },    // Azul
            { "O", Color.FromRgb(255, 13, 13) },    // Vermelho
            { "F", Color.FromRgb(144, 224, 80) },   // Verde claro
            { "Cl", Color.FromRgb(31, 240, 31) },   // Verde
            { "Br", Color.FromRgb(166, 41, 41) },   // Marrom
            { "I", Color.FromRgb(148, 0, 148) },    // Roxo
            { "S", Color.FromRgb(255, 255, 48) },   // Amarelo
            { "P", Color.FromRgb(255, 128, 0) },    // Laranja
            { "Na", Color.FromRgb(171, 92, 242) },  // Violeta
            { "K", Color.FromRgb(143, 64, 212) },   // Violeta escuro
            { "Ca", Color.FromRgb(61, 255, 0) },    // Verde lima
            { "Fe", Color.FromRgb(224, 102, 51) },  // Laranja ferrugem
            { "Cu", Color.FromRgb(200, 128, 51) },  // Cobre
            { "Zn", Color.FromRgb(125, 128, 176) }, // Azul aço
            { "Default", Color.FromRgb(255, 20, 147) } // Rosa (desconhecido)
        };
        
        public MolecularAnimation3DControl()
        {
            InitializeComponent();
            
            // Configurar câmera 3D
            viewport3D.Camera = new PerspectiveCamera
            {
                Position = new Point3D(0, 0, 20),
                LookDirection = new Vector3D(0, 0, -20),
                UpDirection = new Vector3D(0, 1, 0),
                FieldOfView = 60
            };
        }
        
        /// <summary>
        /// Define a reação completa para animação dinâmica
        /// </summary>
        public void SetReaction(Reaction reaction)
        {
            if (reaction == null) return;
            
            // Limpar visualizações antigas
            ClearMolecules();
            
            // Criar representações 3D dos reagentes
            double xPosition = -8;
            foreach (var component in reaction.Reactants)
            {
                var visuals = CreateMoleculeVisualization(component, xPosition, 0);
                _reagentVisuals.AddRange(visuals);
                xPosition += 4;
            }
            
            // Criar representações 3D dos produtos (inicialmente invisíveis)
            xPosition = 4;
            foreach (var component in reaction.Products)
            {
                var visuals = CreateMoleculeVisualization(component, xPosition, 0);
                
                // Esconder produtos inicialmente
                foreach (var visual in visuals)
                {
                    if (visual is SphereVisual3D sphere)
                        sphere.Visible = false;
                }
                
                _productVisuals.AddRange(visuals);
                xPosition += 4;
            }
            
            UpdateAnimationStep("Moléculas carregadas. Clique em INICIAR para animar.");
        }
        
        /// <summary>
        /// Cria visualização 3D de uma molécula com átomos individuais coloridos
        /// </summary>
        private List<Visual3D> CreateMoleculeVisualization(ReactionComponent component, double x, double y)
        {
            var visuals = new List<Visual3D>();
            
            if (component?.Molecule == null) return visuals;
            
            var molecule = component.Molecule;
            string formula = molecule.Formula;
            
            // Extrair elementos da fórmula (ex: H2O → H, H, O)
            var atoms = ParseFormula(formula);
            
            // Criar grupo de átomos em arranjo espacial
            int atomCount = atoms.Count;
            double radius = 0.5; // Raio de cada átomo
            double spacing = 1.2; // Espaçamento entre átomos
            
            for (int i = 0; i < atomCount; i++)
            {
                // Posicionar átomos em formato circular compacto
                double angle = (2 * Math.PI * i) / Math.Max(atomCount, 1);
                double offsetX = i == 0 ? 0 : Math.Cos(angle) * spacing;
                double offsetY = i == 0 ? 0 : Math.Sin(angle) * spacing;
                
                Color atomColor = GetElementColor(atoms[i]);
                
                var sphere = new SphereVisual3D
                {
                    Center = new Point3D(x + offsetX, y + offsetY, 0),
                    Radius = radius,
                    Fill = new SolidColorBrush(atomColor)
                };
                
                viewport3D.Children.Add(sphere);
                visuals.Add(sphere);
            }
            
            // Adicionar rótulo com fórmula e nome
            var label = new BillboardTextVisual3D
            {
                Text = $"{formula}\n{molecule.Name}",
                Position = new Point3D(x, y - 2.5, 0),
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.Bold
            };
            
            viewport3D.Children.Add(label);
            visuals.Add(label);
            
            return visuals;
        }
        
        /// <summary>
        /// Extrai elementos individuais da fórmula química
        /// Ex: H2O → [H, H, O], CH4 → [C, H, H, H, H]
        /// </summary>
        private List<string> ParseFormula(string formula)
        {
            var atoms = new List<string>();
            
            // Remover subscripts Unicode (₂ → 2)
            formula = formula.Replace("₂", "2").Replace("₃", "3").Replace("₄", "4")
                             .Replace("₅", "5").Replace("₆", "6");
            
            for (int i = 0; i < formula.Length; i++)
            {
                if (char.IsUpper(formula[i]))
                {
                    string element = formula[i].ToString();
                    
                    // Verificar se há letra minúscula (ex: Cl, Br)
                    if (i + 1 < formula.Length && char.IsLower(formula[i + 1]))
                    {
                        element += formula[i + 1];
                        i++;
                    }
                    
                    // Verificar quantidade (número subscrito)
                    int count = 1;
                    if (i + 1 < formula.Length && char.IsDigit(formula[i + 1]))
                    {
                        count = int.Parse(formula[i + 1].ToString());
                        i++;
                    }
                    
                    // Adicionar átomos
                    for (int j = 0; j < count; j++)
                    {
                        atoms.Add(element);
                    }
                }
            }
            
            return atoms.Count > 0 ? atoms : new List<string> { "C" }; // Default
        }
        
        /// <summary>
        /// Retorna cor CPK do elemento químico
        /// </summary>
        private Color GetElementColor(string element)
        {
            return ElementColors.TryGetValue(element, out Color color) 
                ? color 
                : ElementColors["Default"];
        }
        
        /// <summary>
        /// Limpa todas as moléculas da cena 3D
        /// </summary>
        private void ClearMolecules()
        {
            foreach (var visual in _reagentVisuals.Concat(_productVisuals))
            {
                viewport3D.Children.Remove(visual);
            }
            _reagentVisuals.Clear();
            _productVisuals.Clear();
        }
        
        /// <summary>
        /// Inicia animação simplificada: reagentes desaparecem, produtos aparecem
        /// </summary>
        public void StartAnimation(double animationSpeed = 1.0)
        {
            StopAnimation();
            
            _currentStoryboard = new Storyboard();
            double duration = 4.0 / animationSpeed; // 4 segundos base
            
            // Fase 1: Esconder reagentes (0-50%)
            foreach (var visual in _reagentVisuals)
            {
                if (visual is SphereVisual3D sphere)
                {
                    var fadeOut = new ObjectAnimationUsingKeyFrames();
                    fadeOut.KeyFrames.Add(new DiscreteObjectKeyFrame(false, 
                        KeyTime.FromTimeSpan(TimeSpan.FromSeconds(duration * 0.5))));
                    Storyboard.SetTarget(fadeOut, sphere);
                    Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Visible"));
                    _currentStoryboard.Children.Add(fadeOut);
                }
            }
            
            // Fase 2: Mostrar produtos (50-100%)
            foreach (var visual in _productVisuals)
            {
                if (visual is SphereVisual3D sphere)
                {
                    var fadeIn = new ObjectAnimationUsingKeyFrames();
                    fadeIn.KeyFrames.Add(new DiscreteObjectKeyFrame(true, 
                        KeyTime.FromTimeSpan(TimeSpan.FromSeconds(duration * 0.5))));
                    Storyboard.SetTarget(fadeIn, sphere);
                    Storyboard.SetTargetProperty(fadeIn, new PropertyPath("Visible"));
                    _currentStoryboard.Children.Add(fadeIn);
                }
            }
            
            UpdateAnimationStep("Animando reação...");
            _currentStoryboard.Completed += (s, e) => UpdateAnimationStep("Animação concluída!");
            _currentStoryboard.Begin();
        }
        
        public void StopAnimation()
        {
            _currentStoryboard?.Stop();
            _currentStoryboard = null;
        }
        
        public void ResetPositions()
        {
            // Mostrar reagentes, esconder produtos
            foreach (var visual in _reagentVisuals)
            {
                if (visual is SphereVisual3D sphere)
                    sphere.Visible = true;
            }
            
            foreach (var visual in _productVisuals)
            {
                if (visual is SphereVisual3D sphere)
                    sphere.Visible = false;
            }
            
            UpdateAnimationStep("Posições resetadas. Clique em INICIAR.");
        }
        
        public void UpdateAnimationStep(string stepDescription)
        {
            AnimationStepText.Text = stepDescription;
        }
    }
}
