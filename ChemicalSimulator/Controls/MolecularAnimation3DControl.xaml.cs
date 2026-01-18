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
        private BillboardTextVisual3D? _reactionArrow; // SETA DE REAÇÃO
        
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
            
            // ADICIONAR SETA DE REAÇÃO NO CENTRO
            _reactionArrow = new BillboardTextVisual3D
            {
                Text = "→",
                Position = new Point3D(0, 0, 0),
                Foreground = new SolidColorBrush(Color.FromRgb(0, 212, 255)), // Ciano brilhante
                FontSize = 48,
                FontWeight = FontWeights.Bold
            };
            viewport3D.Children.Add(_reactionArrow);
            
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
                
                // Criar brush MUTÁVEL para permitir animações de cor
                var brush = new SolidColorBrush(atomColor);
                // NÃO chamar .Freeze() para manter animável!
                
                var sphere = new SphereVisual3D
                {
                    Center = new Point3D(x + offsetX, y + offsetY, 0),
                    Radius = radius,
                    Fill = brush
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
            
            // Remover seta de reação
            if (_reactionArrow != null)
            {
                viewport3D.Children.Remove(_reactionArrow);
                _reactionArrow = null;
            }
        }
        
        /// <summary>
        /// Inicia animação PROFISSIONAL com 5 etapas visíveis + seta de reação
        /// </summary>
        public void StartAnimation(double animationSpeed = 1.0)
        {
            StopAnimation();
            ResetPositions(); // Garantir estado inicial
            
            _currentStoryboard = new Storyboard();
            
            if (_currentStoryboard == null) return; // Safety check
            
            double stepDuration = 2.0 / animationSpeed; // 2 segundos por etapa
            double totalDuration = stepDuration * 5;
            
            // ===== ETAPA 1: Aproximação dos Reagentes (0-20%) =====
            AnimateStep1_Approach(stepDuration * 0);
            
            // ===== ETAPA 2: Colisão e Formação do Complexo Ativado (20-40%) =====
            AnimateStep2_Collision(stepDuration * 1);
            
            // ===== ETAPA 3: Quebra de Ligações Antigas (40-60%) =====
            AnimateStep3_BreakBonds(stepDuration * 2);
            
            // ===== ETAPA 4: Formação de Novas Ligações (60-80%) =====
            AnimateStep4_FormBonds(stepDuration * 3);
            
            // ===== ETAPA 5: Separação dos Produtos (80-100%) =====
            AnimateStep5_Separation(stepDuration * 4);
            
            // Eventos de progresso
            var progressTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            
            DateTime startTime = DateTime.Now;
            progressTimer.Tick += (s, e) =>
            {
                double elapsed = (DateTime.Now - startTime).TotalSeconds;
                double progress = Math.Min(elapsed / totalDuration, 1.0) * 100;
                
                // Atualizar descrição conforme progresso
                if (progress < 20)
                    UpdateAnimationStep("⚛️ ETAPA 1: Aproximação das moléculas");
                else if (progress < 40)
                    UpdateAnimationStep("💥 ETAPA 2: Colisão e formação do complexo ativado");
                else if (progress < 60)
                    UpdateAnimationStep("🔗 ETAPA 3: Quebra das ligações antigas");
                else if (progress < 80)
                    UpdateAnimationStep("✨ ETAPA 4: Formação de novas ligações");
                else if (progress < 100)
                    UpdateAnimationStep("🚀 ETAPA 5: Separação dos produtos");
                else
                {
                    UpdateAnimationStep("✅ Reação completa! Produtos formados.");
                    progressTimer.Stop();
                }
            };
            
            progressTimer.Start();
            _currentStoryboard.Completed += (s, e) => progressTimer.Stop();
            _currentStoryboard.Begin();
        }
        
        /// <summary>
        /// ETAPA 1: Reagentes se aproximam do centro (movimento suave)
        /// </summary>
        private void AnimateStep1_Approach(double startTime)
        {
            foreach (var visual in _reagentVisuals)
            {
                if (visual is SphereVisual3D sphere)
                {
                    var currentPos = sphere.Center;
                    var targetX = currentPos.X > 0 ? -2 : 2; // Aproximar do centro
                    
                    var moveAnimation = new Point3DAnimation
                    {
                        From = currentPos,
                        To = new Point3D(targetX, currentPos.Y, currentPos.Z),
                        Duration = TimeSpan.FromSeconds(2),
                        BeginTime = TimeSpan.FromSeconds(startTime),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                    };
                    
                    Storyboard.SetTarget(moveAnimation, sphere);
                    Storyboard.SetTargetProperty(moveAnimation, new PropertyPath("Center"));
                    _currentStoryboard!.Children.Add(moveAnimation);
                }
            }
        }
        
        /// <summary>
        /// ETAPA 2: Colisão - átomos vibram intensamente
        /// </summary>
        private void AnimateStep2_Collision(double startTime)
        {
            foreach (var visual in _reagentVisuals)
            {
                if (visual is SphereVisual3D sphere)
                {
                    // Vibração rápida (simulando energia cinética)
                    var vibrationX = new DoubleAnimation
                    {
                        From = 0,
                        To = 0.3,
                        Duration = TimeSpan.FromSeconds(0.1),
                        BeginTime = TimeSpan.FromSeconds(startTime),
                        AutoReverse = true,
                        RepeatBehavior = new RepeatBehavior(10) // 10 vibrações
                    };
                    
                    // Aplicar transformação (necessário adicionar TranslateTransform3D)
                    // Por simplificação, vamos aumentar/diminuir o raio para simular vibração
                    var pulseAnimation = new DoubleAnimation
                    {
                        From = sphere.Radius,
                        To = sphere.Radius * 1.3,
                        Duration = TimeSpan.FromSeconds(0.15),
                        BeginTime = TimeSpan.FromSeconds(startTime),
                        AutoReverse = true,
                        RepeatBehavior = new RepeatBehavior(6)
                    };
                    
                    Storyboard.SetTarget(pulseAnimation, sphere);
                    Storyboard.SetTargetProperty(pulseAnimation, new PropertyPath("Radius"));
                    _currentStoryboard!.Children.Add(pulseAnimation);
                }
            }
        }
        
        /// <summary>
        /// ETAPA 3: Quebra de ligações - reagentes encolhem até desaparecer
        /// </summary>
        private void AnimateStep3_BreakBonds(double startTime)
        {
            foreach (var visual in _reagentVisuals)
            {
                if (visual is SphereVisual3D sphere)
                {
                    // Encolher até quase invisível
                    var shrinkAnimation = new DoubleAnimation
                    {
                        From = sphere.Radius,
                        To = 0.05,
                        Duration = TimeSpan.FromSeconds(1.5),
                        BeginTime = TimeSpan.FromSeconds(startTime),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                    };
                    
                    Storyboard.SetTarget(shrinkAnimation, sphere);
                    Storyboard.SetTargetProperty(shrinkAnimation, new PropertyPath("Radius"));
                    _currentStoryboard!.Children.Add(shrinkAnimation);
                }
            }
        }
        
        /// <summary>
        /// ETAPA 4: Formação de ligações - produtos crescem do nada
        /// </summary>
        private void AnimateStep4_FormBonds(double startTime)
        {
            foreach (var visual in _productVisuals)
            {
                if (visual is SphereVisual3D sphere)
                {
                    sphere.Visible = true; // Garantir visibilidade
                    sphere.Radius = 0.05; // Começar minúsculo
                    
                    // Crescer do nada com efeito "pop"
                    var growAnimation = new DoubleAnimation
                    {
                        From = 0.05,
                        To = 0.5,
                        Duration = TimeSpan.FromSeconds(1.5),
                        BeginTime = TimeSpan.FromSeconds(startTime),
                        EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.5 }
                    };
                    
                    Storyboard.SetTarget(growAnimation, sphere);
                    Storyboard.SetTargetProperty(growAnimation, new PropertyPath("Radius"));
                    _currentStoryboard!.Children.Add(growAnimation);
                }
            }
        }
        
        /// <summary>
        /// ETAPA 5: Produtos se afastam para posições finais
        /// </summary>
        private void AnimateStep5_Separation(double startTime)
        {
            foreach (var visual in _productVisuals)
            {
                if (visual is SphereVisual3D sphere)
                {
                    var currentPos = sphere.Center;
                    var targetX = currentPos.X < 0 ? currentPos.X - 2 : currentPos.X + 2;
                    
                    var separateAnimation = new Point3DAnimation
                    {
                        To = new Point3D(targetX, currentPos.Y, currentPos.Z),
                        Duration = TimeSpan.FromSeconds(2),
                        BeginTime = TimeSpan.FromSeconds(startTime),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };
                    
                    Storyboard.SetTarget(separateAnimation, sphere);
                    Storyboard.SetTargetProperty(separateAnimation, new PropertyPath("Center"));
                    _currentStoryboard!.Children.Add(separateAnimation);
                }
            }
            
            // Esconder completamente os reagentes no final
            var hideReagentsAnimation = new ObjectAnimationUsingKeyFrames();
            hideReagentsAnimation.KeyFrames.Add(new DiscreteObjectKeyFrame(false, 
                KeyTime.FromTimeSpan(TimeSpan.FromSeconds(startTime + 0.5))));
            
            foreach (var visual in _reagentVisuals)
            {
                if (visual is SphereVisual3D sphere)
                {
                    var hideAnim = new ObjectAnimationUsingKeyFrames();
                    hideAnim.KeyFrames.Add(new DiscreteObjectKeyFrame(false, 
                        KeyTime.FromTimeSpan(TimeSpan.FromSeconds(startTime + 0.5))));
                    Storyboard.SetTarget(hideAnim, sphere);
                    Storyboard.SetTargetProperty(hideAnim, new PropertyPath("Visible"));
                    _currentStoryboard!.Children.Add(hideAnim);
                }
            }
        }
        
        public void StopAnimation()
        {
            _currentStoryboard?.Stop();
            _currentStoryboard = null;
        }
        
        public void ResetPositions()
        {
            // Parar animação se estiver rodando
            StopAnimation();
            
            // Mostrar reagentes com cor e tamanho originais
            foreach (var visual in _reagentVisuals)
            {
                if (visual is SphereVisual3D sphere)
                {
                    sphere.Visible = true;
                    sphere.Radius = 0.5; // Tamanho original
                    
                    // Restaurar cor original (opaca)
                    var currentColor = ((SolidColorBrush)sphere.Fill).Color;
                    sphere.Fill = new SolidColorBrush(Color.FromArgb(255, currentColor.R, currentColor.G, currentColor.B));
                }
            }
            
            // Esconder produtos completamente
            foreach (var visual in _productVisuals)
            {
                if (visual is SphereVisual3D sphere)
                {
                    sphere.Visible = false;
                    sphere.Radius = 0.5; // Preparar para próxima animação
                }
            }
            
            UpdateAnimationStep("Posições resetadas. Clique em INICIAR.");
        }
        
        public void UpdateAnimationStep(string stepDescription)
        {
            AnimationStepText.Text = stepDescription;
        }
    }
}
