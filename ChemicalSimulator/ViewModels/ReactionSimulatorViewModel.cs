using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ChemicalSimulator.Commands;
using ChemicalSimulator.Models;
using ChemicalSimulator.Services;
using System.ComponentModel;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using ModelElement = ChemicalSimulator.Models.Element;

namespace ChemicalSimulator.ViewModels
{
    /// <summary>
    /// ViewModel AVANÇADA para simulação profissional de reações químicas
    /// Com funcionalidades de predição, análise termodinâmica e cinética em tempo real
    /// </summary>
    public class ReactionSimulatorViewModel : ViewModelBase
    {
        #region Services
        private readonly ReactionPredictor _reactionPredictor;
        private readonly ChemistryEngine _chemistryEngine;
        private readonly ElementDataLoader _elementDataLoader;
        private readonly CompoundDataLoader _compoundDataLoader;
        private readonly DispatcherTimer _animationTimer;
        private readonly DispatcherTimer _reactionProgressTimer;
        
        // 🆕 NOVO: Serviço profissional de predição química
        private readonly Services.Chemistry.ReactionPredictionService _predictionService;
        #endregion

        #region Properties - Compostos Disponíveis
        private ObservableCollection<Compound> _availableCompounds = new();
        public ObservableCollection<Compound> AvailableCompounds
        {
            get => _availableCompounds;
            set => SetProperty(ref _availableCompounds, value);
        }

        private ObservableCollection<ModelElement> _availableElements = new();
        public ObservableCollection<ModelElement> AvailableElements
        {
            get => _availableElements;
            set => SetProperty(ref _availableElements, value);
        }
        #endregion

        #region Properties - Reagentes e Produtos
        private ObservableCollection<ReactionComponent> _reactants = new();
        public ObservableCollection<ReactionComponent> Reactants
        {
            get => _reactants;
            set => SetProperty(ref _reactants, value);
        }

        private ObservableCollection<ReactionComponent> _products = new();
        public ObservableCollection<ReactionComponent> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        private Compound? _selectedReactant1;
        public Compound? SelectedReactant1
        {
            get => _selectedReactant1;
            set
            {
                if (SetProperty(ref _selectedReactant1, value))
                {
                    System.Diagnostics.Debug.WriteLine($"🔄 SelectedReactant1 alterado para: {value?.Name ?? "NULL"}");
                    UpdateReaction();
                    
                    // Forçar reavaliação dos comandos
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private Compound? _selectedReactant2;
        public Compound? SelectedReactant2
        {
            get => _selectedReactant2;
            set
            {
                if (SetProperty(ref _selectedReactant2, value))
                {
                    System.Diagnostics.Debug.WriteLine($"🔄 SelectedReactant2 alterado para: {value?.Name ?? "NULL"}");
                    UpdateReaction();
                    
                    // Forçar reavaliação dos comandos
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private Compound? _selectedProduct;
        public Compound? SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
        }
        #endregion

        #region Properties - Condições da Reação
        private double _temperature = 298.15; // 25°C em Kelvin
        public double Temperature
        {
            get => _temperature;
            set
            {
                if (SetProperty(ref _temperature, value))
                {
                    OnPropertyChanged(nameof(TemperatureCelsius));
                    OnPropertyChanged(nameof(TemperatureDisplay));
                    UpdateThermodynamics();
                }
            }
        }

        public double TemperatureCelsius
        {
            get => _temperature - 273.15;
            set => Temperature = value + 273.15;
        }

        public string TemperatureDisplay => $"{TemperatureCelsius:F1}°C ({Temperature:F1}K)";

        private double _pressure = 101.325; // 1 atm em kPa
        public double Pressure
        {
            get => _pressure;
            set
            {
                if (SetProperty(ref _pressure, value))
                {
                    OnPropertyChanged(nameof(PressureDisplay));
                    UpdateThermodynamics();
                }
            }
        }

        public string PressureDisplay => $"{Pressure:F2} kPa ({Pressure / 101.325:F2} atm)";

        private string _catalyst = string.Empty;
        public string Catalyst
        {
            get => _catalyst;
            set
            {
                if (SetProperty(ref _catalyst, value))
                {
                    UpdateKinetics();
                }
            }
        }

        private bool _usesCatalyst;
        public bool UsesCatalyst
        {
            get => _usesCatalyst;
            set
            {
                if (SetProperty(ref _usesCatalyst, value))
                {
                    if (!value) Catalyst = string.Empty;
                    UpdateKinetics();
                }
            }
        }
        #endregion

        #region Properties - Equação Química
        private string _balancedEquation = string.Empty;
        public string BalancedEquation
        {
            get => _balancedEquation;
            set => SetProperty(ref _balancedEquation, value);
        }

        private bool _isBalanced;
        public bool IsBalanced
        {
            get => _isBalanced;
            set => SetProperty(ref _isBalanced, value);
        }

        private string _reactionTypeText = string.Empty;
        public string ReactionTypeText
        {
            get => _reactionTypeText;
            set => SetProperty(ref _reactionTypeText, value);
        }
        
        private Reaction? _currentReaction;
        public Reaction? CurrentReaction
        {
            get => _currentReaction;
            set => SetProperty(ref _currentReaction, value);
        }
        #endregion

        #region Properties - Cálculos Termodinâmicos
        private double _enthalpyChange;
        public double EnthalpyChange
        {
            get => _enthalpyChange;
            set
            {
                if (SetProperty(ref _enthalpyChange, value))
                {
                    OnPropertyChanged(nameof(IsExothermic));
                    OnPropertyChanged(nameof(EnergyTypeText));
                }
            }
        }

        public bool IsExothermic => EnthalpyChange < 0;
        public string EnergyTypeText => IsExothermic ? "EXOTÉRMICA" : "ENDOTÉRMICA";

        private double _gibbsFreeEnergy;
        public double GibbsFreeEnergy
        {
            get => _gibbsFreeEnergy;
            set
            {
                if (SetProperty(ref _gibbsFreeEnergy, value))
                {
                    OnPropertyChanged(nameof(IsSpontaneous));
                }
            }
        }

        public bool IsSpontaneous => GibbsFreeEnergy < 0;

        private double _activationEnergy;
        public double ActivationEnergy
        {
            get => _activationEnergy;
            set => SetProperty(ref _activationEnergy, value);
        }
        #endregion

        #region Properties - Reagente Limitante
        private double _reactant1Amount = 1.0;
        public double Reactant1Amount
        {
            get => _reactant1Amount;
            set
            {
                if (SetProperty(ref _reactant1Amount, value))
                {
                    CalculateLimitingReagent();
                }
            }
        }

        private double _reactant2Amount = 1.0;
        public double Reactant2Amount
        {
            get => _reactant2Amount;
            set
            {
                if (SetProperty(ref _reactant2Amount, value))
                {
                    CalculateLimitingReagent();
                }
            }
        }

        private string _limitingReagent = string.Empty;
        public string LimitingReagent
        {
            get => _limitingReagent;
            set => SetProperty(ref _limitingReagent, value);
        }

        private double _theoreticalYield;
        public double TheoreticalYield
        {
            get => _theoreticalYield;
            set => SetProperty(ref _theoreticalYield, value);
        }

        private double _percentYield = 100;
        public double PercentYield
        {
            get => _percentYield;
            set => SetProperty(ref _percentYield, value);
        }
        #endregion

        #region Properties - Animação
        private bool _isAnimating;
        public bool IsAnimating
        {
            get => _isAnimating;
            set => SetProperty(ref _isAnimating, value);
        }

        private double _animationProgress;
        public double AnimationProgress
        {
            get => _animationProgress;
            set
            {
                if (SetProperty(ref _animationProgress, value))
                {
                    UpdateAnimationFrame();
                }
            }
        }

        private int _currentAnimationStep;
        public int CurrentAnimationStep
        {
            get => _currentAnimationStep;
            set => SetProperty(ref _currentAnimationStep, value);
        }

        private string _animationStepDescription = string.Empty;
        public string AnimationStepDescription
        {
            get => _animationStepDescription;
            set => SetProperty(ref _animationStepDescription, value);
        }

        private string _animationSpeed = "Normal";
        public string AnimationSpeed
        {
            get => _animationSpeed;
            set
            {
                if (SetProperty(ref _animationSpeed, value))
                {
                    UpdateAnimationTimerSpeed();
                }
            }
        }

        private ObservableCollection<string> _animationSteps = new();
        public ObservableCollection<string> AnimationSteps
        {
            get => _animationSteps;
            set => SetProperty(ref _animationSteps, value);
        }
        #endregion

        #region Properties - Visualização de Energia
        private PlotModel _energyDiagram = new();
        public PlotModel EnergyDiagram
        {
            get => _energyDiagram;
            set => SetProperty(ref _energyDiagram, value);
        }
        #endregion

        #region Commands
        public ICommand AddReactantCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand RemoveReactantCommand { get; }
        public ICommand RemoveProductCommand { get; }
        public ICommand BalanceEquationCommand { get; }
        public ICommand StartAnimationCommand { get; }
        public ICommand PauseAnimationCommand { get; }
        public ICommand ResetAnimationCommand { get; }
        public ICommand PredictProductsCommand { get; }
        public ICommand AutoBalanceCommand { get; }
        #endregion

        #region Constructor
        public ReactionSimulatorViewModel()
        {
            // Inicializar serviços
            _elementDataLoader = new ElementDataLoader();
            _compoundDataLoader = new CompoundDataLoader();
            _chemistryEngine = new ChemistryEngine();
            _reactionPredictor = new ReactionPredictor();
            
            // 🆕 NOVO: Inicializar serviço profissional de predição
            _predictionService = new Services.Chemistry.ReactionPredictionService();
            System.Diagnostics.Debug.WriteLine("✅ ReactionPredictionService inicializado!");

            // Carregar dados
            LoadChemicalData();

            // Inicializar comandos
            AddReactantCommand = new RelayCommand(AddReactant, CanAddReactant);
            AddProductCommand = new RelayCommand(AddProduct, CanAddProduct);
            RemoveReactantCommand = new RelayCommand<ReactionComponent>(RemoveReactant!);
            RemoveProductCommand = new RelayCommand<ReactionComponent>(RemoveProduct!);
            BalanceEquationCommand = new RelayCommand(BalanceEquation, () => Reactants.Any() && Products.Any());
            StartAnimationCommand = new RelayCommand(StartAnimation, CanStartAnimation);
            PauseAnimationCommand = new RelayCommand(PauseAnimation, () => IsAnimating);
            ResetAnimationCommand = new RelayCommand(ResetAnimation);
            PredictProductsCommand = new RelayCommand(PredictProducts, CanPredictProducts);
            AutoBalanceCommand = new RelayCommand(AutoBalance, CanAutoBalance);

            // Inicializar timers de animação
            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            _animationTimer.Tick += AnimationTimer_Tick!;

            _reactionProgressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _reactionProgressTimer.Tick += ReactionProgressTimer_Tick!;

            // Criar diagrama de energia inicial
            CreateEnergyDiagram();
        }
        #endregion

        #region Data Loading
        private void LoadChemicalData()
        {
            try
            {
                // Carregar elementos
                var elements = _elementDataLoader.LoadElements();
                AvailableElements = new ObservableCollection<ModelElement>(elements);

                // Carregar compostos
                var compounds = _compoundDataLoader.LoadCompounds();
                AvailableCompounds = new ObservableCollection<Compound>(compounds);

                System.Diagnostics.Debug.WriteLine($"✅ Carregados {AvailableElements.Count} elementos e {AvailableCompounds.Count} compostos");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao carregar dados químicos: {ex.Message}");
                
                // Fallback para dados mínimos
                AvailableElements = new ObservableCollection<ModelElement>();
                AvailableCompounds = new ObservableCollection<Compound>();
            }
        }
        #endregion

        #region Reaction Management
        private void UpdateReaction()
        {
            System.Diagnostics.Debug.WriteLine("🔄 UpdateReaction() chamado!");
            
            if (SelectedReactant1 == null)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ SelectedReactant1 é null, saindo...");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"✅ Atualizando reação com: {SelectedReactant1.Name}");

            // Limpar produtos
            Products.Clear();

            // Atualizar reagentes
            Reactants.Clear();
            Reactants.Add(new ReactionComponent
            {
                Molecule = ConvertCompoundToMolecule(SelectedReactant1),
                Coefficient = 1,
                Phase = DeterminePhase(SelectedReactant1)
            });

            if (SelectedReactant2 != null)
            {
                System.Diagnostics.Debug.WriteLine($"✅ Adicionando reagente 2: {SelectedReactant2.Name}");
                Reactants.Add(new ReactionComponent
                {
                    Molecule = ConvertCompoundToMolecule(SelectedReactant2),
                    Coefficient = 1,
                    Phase = DeterminePhase(SelectedReactant2)
                });
            }

            UpdateBalancedEquation();
            CalculateLimitingReagent();
            
            // Criar objeto Reaction para animação 3D
            CurrentReaction = new Reaction
            {
                Reactants = new List<ReactionComponent>(Reactants),
                Products = new List<ReactionComponent>(Products)
            };
            
            System.Diagnostics.Debug.WriteLine($"📊 Reagentes atualizados: {Reactants.Count}");
        }

        private void PredictProducts()
        {
            System.Diagnostics.Debug.WriteLine("🔮 PredictProducts() chamado!");
            
            if (SelectedReactant1 == null)
            {
                System.Diagnostics.Debug.WriteLine("❌ SelectedReactant1 é null!");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"✅ Reagente 1: {SelectedReactant1.Name} ({SelectedReactant1.Formula})");
            if (SelectedReactant2 != null)
            {
                System.Diagnostics.Debug.WriteLine($"✅ Reagente 2: {SelectedReactant2.Name} ({SelectedReactant2.Formula})");
            }

            Products.Clear();

            // Predição baseada em padrões de reação
            var predictedProducts = PredictProductsBasedOnReactants();

            System.Diagnostics.Debug.WriteLine($"📦 Produtos previstos: {predictedProducts.Count}");

            foreach (var product in predictedProducts)
            {
                Products.Add(product);
                System.Diagnostics.Debug.WriteLine($"  ➕ Adicionado: {product.Molecule.Name}");
            }

            AutoBalance();
            UpdateThermodynamics();
            
            // Atualizar CurrentReaction para animação 3D
            CurrentReaction = new Reaction
            {
                Reactants = new List<ReactionComponent>(Reactants),
                Products = new List<ReactionComponent>(Products)
            };
            
            System.Diagnostics.Debug.WriteLine($"✅ Predição completa! Total produtos: {Products.Count}");
        }

        private List<ReactionComponent> PredictProductsBasedOnReactants()
        {
            var products = new List<ReactionComponent>();

            if (SelectedReactant1 == null)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ PredictProductsBasedOnReactants: SelectedReactant1 é null!");
                return products;
            }

            System.Diagnostics.Debug.WriteLine($"🔍 🆕 USANDO NOVO MOTOR DE REGRAS QUÍMICAS!");
            System.Diagnostics.Debug.WriteLine($"   Analisando: {SelectedReactant1.Formula}" + 
                (SelectedReactant2 != null ? $" + {SelectedReactant2.Formula}" : ""));

            // 🆕 Preparar lista de reagentes para o novo serviço
            var reactantCompounds = new List<Compound> { SelectedReactant1 };
            if (SelectedReactant2 != null)
            {
                reactantCompounds.Add(SelectedReactant2);
            }

            // 🆕 Executar predição com motor de regras
            var conditions = new Services.Chemistry.ReactionConditions
            {
                Temperature = Temperature,
                Pressure = Pressure,
                Catalyst = Catalyst
            };

            var result = _predictionService.PredictProducts(reactantCompounds, conditions);

            System.Diagnostics.Debug.WriteLine($"📊 Resultado: {result.Message}");
            System.Diagnostics.Debug.WriteLine($"   Tipo: {result.ReactionType}");
            System.Diagnostics.Debug.WriteLine($"   Produtos: {result.Products.Count}");

            // 🆕 Atualizar propriedades termodinâmicas
            ReactionTypeText = GetReactionTypeName(result.ReactionType);
            ActivationEnergy = result.ActivationEnergy;
            EnthalpyChange = result.EnthalpyChange;

            // 🆕 Converter MoleculeGraph → ReactionComponent
            foreach (var productGraph in result.Products)
            {
                var compound = _predictionService.ConvertToCompound(productGraph);
                
                products.Add(new ReactionComponent
                {
                    Molecule = ConvertCompoundToMolecule(compound),
                    Coefficient = 1,
                    Phase = DeterminePhase(compound)
                });

                System.Diagnostics.Debug.WriteLine($"  ✅ Produto: {compound.Formula} ({compound.Name})");
            }

            if (!result.Success)
            {
                System.Diagnostics.Debug.WriteLine($"❌ {result.Message}");
            }

            return products;
        }

        /// <summary>
        /// Converte ReactionType enum em texto amigável
        /// </summary>
        private string GetReactionTypeName(Services.Chemistry.ReactionType type)
        {
            return type switch
            {
                Services.Chemistry.ReactionType.Combustion => "🔥 Reação de Combustão",
                Services.Chemistry.ReactionType.Neutralization => "⚗️ Reação de Neutralização (Ácido-Base)",
                Services.Chemistry.ReactionType.Synthesis => "🧪 Reação de Síntese",
                Services.Chemistry.ReactionType.Decomposition => "💥 Reação de Decomposição",
                Services.Chemistry.ReactionType.SingleDisplacement => "🔄 Reação de Simples Troca",
                Services.Chemistry.ReactionType.DoubleDisplacement => "↔️ Reação de Dupla Troca",
                Services.Chemistry.ReactionType.Redox => "⚡ Reação de Oxirredução",
                Services.Chemistry.ReactionType.Addition => "➕ Reação de Adição",
                Services.Chemistry.ReactionType.Substitution => "🔀 Reação de Substituição",
                Services.Chemistry.ReactionType.NoReaction => "⚠️ Reagentes Incompatíveis - Reação Não Ocorre",
                _ => "Reação Genérica"
            };
        }

        // ========== MÉTODOS AUXILIARES (mantidos para compatibilidade) ==========

        private bool IsAcid(Compound compound)
        {
            return compound.Formula.StartsWith("H") && 
                   (compound.Formula.Contains("Cl") || compound.Formula.Contains("SO4") || 
                    compound.Formula.Contains("NO3") || compound.Formula.Contains("PO4"));
        }

        private bool IsBase(Compound compound)
        {
            return compound.Formula.Contains("OH") || 
                   compound.Name.Contains("hidróxido", StringComparison.OrdinalIgnoreCase);
        }

        private Molecule ConvertCompoundToMolecule(Compound compound)
        {
            return new Molecule
            {
                Name = compound.Name,
                Formula = compound.Formula,
                MolarMass = compound.MolarMass,
                EnthalpyOfFormation = 0, // Compound não tem EnthalpyOfFormation
                MeltingPoint = compound.MeltingPoint ?? 273.15,
                BoilingPoint = compound.BoilingPoint ?? 373.15
            };
        }

        private string DeterminePhase(Compound compound)
        {
            if (Temperature < compound.MeltingPoint)
                return "(s)";
            else if (Temperature < compound.BoilingPoint)
                return "(l)";
            else
                return "(g)";
        }
        #endregion

        #region Balancing
        private void AutoBalance()
        {
            if (!Reactants.Any() || !Products.Any()) return;

            // Algoritmo de balanceamento simplificado
            // Aqui você pode implementar um algoritmo mais sofisticado

            BalanceEquation();
        }

        private void BalanceEquation()
        {
            // Implementação simplificada
            // Em produção, use um algoritmo matricial para balancear

            IsBalanced = true;
            UpdateBalancedEquation();
            UpdateThermodynamics();
        }

        private void UpdateBalancedEquation()
        {
            var reactantsText = string.Join(" + ", 
                Reactants.Select(r => $"{(r.Coefficient > 1 ? r.Coefficient + " " : "")}{r.Molecule.Formula}{r.Phase}"));

            var productsText = string.Join(" + ", 
                Products.Select(p => $"{(p.Coefficient > 1 ? p.Coefficient + " " : "")}{p.Molecule.Formula}{p.Phase}"));

            BalancedEquation = $"{reactantsText} → {productsText}";

            if (!string.IsNullOrEmpty(Catalyst))
            {
                BalancedEquation = $"{reactantsText} →[{Catalyst}] {productsText}";
            }
        }
        #endregion

        #region Thermodynamics
        private void UpdateThermodynamics()
        {
            if (!Reactants.Any() || !Products.Any()) return;

            // Calcular ΔH
            double productsEnthalpy = Products.Sum(p => p.Coefficient * p.Molecule.EnthalpyOfFormation);
            double reactantsEnthalpy = Reactants.Sum(r => r.Coefficient * r.Molecule.EnthalpyOfFormation);
            EnthalpyChange = productsEnthalpy - reactantsEnthalpy;

            // Calcular ΔG (simplificado)
            double entropyChange = -50; // J/(mol·K) - valor estimado
            GibbsFreeEnergy = EnthalpyChange - (Temperature * entropyChange / 1000.0);

            // Atualizar diagrama de energia
            CreateEnergyDiagram();
        }

        private void UpdateKinetics()
        {
            // Aplicar efeito do catalisador
            if (UsesCatalyst && !string.IsNullOrEmpty(Catalyst))
            {
                ActivationEnergy *= 0.6; // Catalisador reduz Ea em ~40%
            }

            CreateEnergyDiagram();
        }

        private void CreateEnergyDiagram()
        {
            var model = new PlotModel
            {
                Title = "Diagrama de Energia da Reação",
                Background = OxyColors.Transparent,
                TitleColor = OxyColors.White,
                TextColor = OxyColors.White,
                PlotAreaBorderColor = OxyColors.Gray
            };

            // Eixos
            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Energia (kJ/mol)",
                TitleColor = OxyColors.White,
                TextColor = OxyColors.White,
                TicklineColor = OxyColors.Gray,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.FromArgb(50, 255, 255, 255)
            });

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Progresso da Reação",
                TitleColor = OxyColors.White,
                TextColor = OxyColors.White,
                TicklineColor = OxyColors.Gray,
                Minimum = 0,
                Maximum = 100
            });

            // Curva de energia
            var energySeries = new LineSeries
            {
                Color = IsExothermic ? OxyColors.OrangeRed : OxyColors.DodgerBlue,
                StrokeThickness = 3,
                MarkerType = MarkerType.Circle,
                MarkerSize = 5,
                MarkerFill = OxyColors.White
            };

            // Reagentes
            energySeries.Points.Add(new DataPoint(0, 0));
            energySeries.Points.Add(new DataPoint(10, 0));

            // Estado de transição (pico)
            energySeries.Points.Add(new DataPoint(50, ActivationEnergy));

            // Produtos
            double productEnergy = EnthalpyChange;
            energySeries.Points.Add(new DataPoint(90, productEnergy));
            energySeries.Points.Add(new DataPoint(100, productEnergy));

            model.Series.Add(energySeries);

            // Anotações
            model.Annotations.Add(new OxyPlot.Annotations.TextAnnotation
            {
                Text = "Reagentes",
                TextPosition = new DataPoint(5, 5),
                TextColor = OxyColors.White
            });

            model.Annotations.Add(new OxyPlot.Annotations.TextAnnotation
            {
                Text = $"Ea = {ActivationEnergy:F1} kJ/mol",
                TextPosition = new DataPoint(50, ActivationEnergy + 10),
                TextColor = OxyColors.Yellow
            });

            model.Annotations.Add(new OxyPlot.Annotations.TextAnnotation
            {
                Text = "Produtos",
                TextPosition = new DataPoint(95, productEnergy + 5),
                TextColor = OxyColors.White
            });

            model.Annotations.Add(new OxyPlot.Annotations.TextAnnotation
            {
                Text = $"ΔH = {EnthalpyChange:F1} kJ/mol",
                TextPosition = new DataPoint(70, (productEnergy) / 2),
                TextColor = IsExothermic ? OxyColors.OrangeRed : OxyColors.DodgerBlue,
                FontWeight = OxyPlot.FontWeights.Bold
            });

            EnergyDiagram = model;
        }
        #endregion

        #region Limiting Reagent
        private void CalculateLimitingReagent()
        {
            if (Reactants.Count < 2) return;

            var r1 = Reactants[0];
            var r2 = Reactants.Count > 1 ? Reactants[1] : null;

            if (r2 == null) return;

            // Calcular mols
            double mols1 = Reactant1Amount / r1.Molecule.MolarMass;
            double mols2 = Reactant2Amount / r2.Molecule.MolarMass;

            // Razão estequiométrica
            double ratio1 = mols1 / r1.Coefficient;
            double ratio2 = mols2 / r2.Coefficient;

            if (ratio1 < ratio2)
            {
                LimitingReagent = r1.Molecule.Name;
                CalculateTheoreticalYield(mols1, r1.Coefficient);
            }
            else
            {
                LimitingReagent = r2.Molecule.Name;
                CalculateTheoreticalYield(mols2, r2.Coefficient);
            }
        }

        private void CalculateTheoreticalYield(double limitingMols, double limitingCoeff)
        {
            if (!Products.Any()) return;

            var mainProduct = Products[0];
            double productMols = limitingMols * (mainProduct.Coefficient / limitingCoeff);
            TheoreticalYield = productMols * mainProduct.Molecule.MolarMass;
        }
        #endregion

        #region Animation
        private void StartAnimation()
        {
            IsAnimating = true;
            AnimationProgress = 0;
            CurrentAnimationStep = 0;

            // Criar etapas da animação
            AnimationSteps.Clear();
            AnimationSteps.Add("1. Aproximação das moléculas");
            AnimationSteps.Add("2. Formação do complexo ativado");
            AnimationSteps.Add("3. Quebra das ligações antigas");
            AnimationSteps.Add("4. Formação de novas ligações");
            AnimationSteps.Add("5. Separação dos produtos");

            _animationTimer.Start();
            _reactionProgressTimer.Start();
        }

        private void PauseAnimation()
        {
            IsAnimating = false;
            _animationTimer.Stop();
            _reactionProgressTimer.Stop();
        }

        private void ResetAnimation()
        {
            IsAnimating = false;
            AnimationProgress = 0;
            CurrentAnimationStep = 0;
            AnimationStepDescription = string.Empty;
            _animationTimer.Stop();
            _reactionProgressTimer.Stop();
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            AnimationProgress += GetAnimationSpeedMultiplier();

            if (AnimationProgress >= 100)
            {
                AnimationProgress = 100;
                PauseAnimation();
            }
        }

        private void ReactionProgressTimer_Tick(object sender, EventArgs e)
        {
            // Atualizar etapa atual baseada no progresso
            int newStep = (int)(AnimationProgress / 20); // 5 etapas
            if (newStep != CurrentAnimationStep && newStep < AnimationSteps.Count)
            {
                CurrentAnimationStep = newStep;
                AnimationStepDescription = AnimationSteps[CurrentAnimationStep];
            }
        }

        private void UpdateAnimationFrame()
        {
            // Aqui você pode atualizar posições de partículas, cores, etc.
            // Será usado pela View para animar elementos visuais
        }

        private double GetAnimationSpeedMultiplier()
        {
            return AnimationSpeed switch
            {
                "Lenta" => 0.5,
                "Normal" => 1.0,
                "Rápida" => 2.0,
                "Instantânea" => 100.0,
                _ => 1.0
            };
        }

        private void UpdateAnimationTimerSpeed()
        {
            // A velocidade é controlada pelo multiplicador em GetAnimationSpeedMultiplier
        }
        #endregion

        #region Command Handlers
        private void AddReactant()
        {
            if (SelectedReactant1 != null)
            {
                Reactants.Add(new ReactionComponent
                {
                    Molecule = ConvertCompoundToMolecule(SelectedReactant1),
                    Coefficient = 1,
                    Phase = DeterminePhase(SelectedReactant1)
                });
                UpdateBalancedEquation();
            }
        }

        private bool CanAddReactant() => SelectedReactant1 != null;

        private void AddProduct()
        {
            if (SelectedProduct != null)
            {
                Products.Add(new ReactionComponent
                {
                    Molecule = ConvertCompoundToMolecule(SelectedProduct),
                    Coefficient = 1,
                    Phase = DeterminePhase(SelectedProduct)
                });
                UpdateBalancedEquation();
                UpdateThermodynamics();
            }
        }

        private bool CanAddProduct() => SelectedProduct != null;

        private void RemoveReactant(ReactionComponent reactant)
        {
            Reactants.Remove(reactant);
            UpdateBalancedEquation();
        }

        private void RemoveProduct(ReactionComponent product)
        {
            Products.Remove(product);
            UpdateBalancedEquation();
            UpdateThermodynamics();
        }

        private bool CanStartAnimation() => Reactants.Any() && Products.Any() && !IsAnimating;

        private bool CanPredictProducts() => SelectedReactant1 != null;

        private bool CanAutoBalance() => Reactants.Any() && Products.Any();
        #endregion
    }
}
