using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ChemicalSimulator.Models;
using ModelElement = ChemicalSimulator.Models.Element;

namespace ChemicalSimulator.Views
{
    /// <summary>
    /// Dialog para construir moléculas customizadas a partir de elementos
    /// </summary>
    public partial class MoleculeBuilderDialog : Window
    {
        private readonly List<ModelElement> _availableElements;
        private readonly ObservableCollection<AtomViewModel> _selectedAtoms = new();
        private Dictionary<string, int> _atomCounts = new();

        public Compound? CreatedCompound { get; private set; }

        public MoleculeBuilderDialog(List<ModelElement> elements)
        {
            InitializeComponent();
            _availableElements = elements;
            
            LoadElements();
            SelectedAtomsPanel.ItemsSource = _selectedAtoms;
        }

        /// <summary>
        /// Carrega elementos mais comuns na tabela periódica simplificada
        /// </summary>
        private void LoadElements()
        {
            // Elementos mais usados em química (ordem de importância)
            var commonElements = new[]
            {
                "H", "C", "N", "O", "S", "P",      // Orgânicos
                "F", "Cl", "Br", "I",              // Halogênios
                "Na", "K", "Ca", "Mg",             // Metais alcalinos/alcalino-terrosos
                "Fe", "Cu", "Zn", "Ag",            // Metais de transição
                "Al", "Si"                          // Outros
            };

            foreach (var symbol in commonElements)
            {
                var element = _availableElements.FirstOrDefault(e => e.Symbol == symbol);
                if (element != null)
                {
                    var button = CreateElementButton(element);
                    ElementsPanel.Children.Add(button);
                }
            }
        }

        /// <summary>
        /// Cria botão visual para um elemento
        /// </summary>
        private Button CreateElementButton(ModelElement element)
        {
            var button = new Button
            {
                Content = element.Symbol,
                ToolTip = element.Name,
                Tag = element.AtomicNumber.ToString(),
                Style = (Style)FindResource("ElementButtonStyle"),
                Background = new SolidColorBrush(GetElementColor(element.Symbol)),
                BorderBrush = new SolidColorBrush(Colors.Transparent)
            };

            button.Click += (s, e) => AddAtom(element);

            return button;
        }

        /// <summary>
        /// Adiciona um átomo à molécula
        /// </summary>
        private void AddAtom(ModelElement element)
        {
            // Atualizar contador
            if (!_atomCounts.ContainsKey(element.Symbol))
            {
                _atomCounts[element.Symbol] = 0;
            }
            _atomCounts[element.Symbol]++;

            System.Diagnostics.Debug.WriteLine($"➕ Adicionado: {element.Symbol} (total: {_atomCounts[element.Symbol]})");
            
            UpdateMoleculeView();
        }

        /// <summary>
        /// Remove um átomo (clique direito)
        /// </summary>
        private void SelectedAtom_RightClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is AtomViewModel atom)
            {
                if (_atomCounts.ContainsKey(atom.Symbol) && _atomCounts[atom.Symbol] > 0)
                {
                    _atomCounts[atom.Symbol]--;
                    
                    System.Diagnostics.Debug.WriteLine($"➖ Removido: {atom.Symbol} (restam: {_atomCounts[atom.Symbol]})");
                    
                    if (_atomCounts[atom.Symbol] == 0)
                    {
                        _atomCounts.Remove(atom.Symbol);
                        System.Diagnostics.Debug.WriteLine($"   🗑️ {atom.Symbol} zerado - removido do dicionário");
                    }
                    
                    UpdateMoleculeView();
                }
            }
        }

        /// <summary>
        /// Atualiza a visualização da molécula
        /// </summary>
        private void UpdateMoleculeView()
        {
            _selectedAtoms.Clear();

            // Usar a mesma ordenação da fórmula
            var ordered = GetOrderedAtoms();

            System.Diagnostics.Debug.WriteLine($"🔄 UpdateMoleculeView: {_atomCounts.Count} elementos diferentes");

            foreach (var kvp in ordered)
            {
                var element = _availableElements.First(e => e.Symbol == kvp.Key);
                
                _selectedAtoms.Add(new AtomViewModel
                {
                    Symbol = kvp.Key,
                    Count = kvp.Value,
                    Color = new SolidColorBrush(GetElementColor(kvp.Key)),
                    CountVisibility = kvp.Value > 1 ? Visibility.Visible : Visibility.Collapsed
                });

                System.Diagnostics.Debug.WriteLine($"   ✅ {kvp.Key}: {kvp.Value} átomo(s) - Badge: {(kvp.Value > 1 ? "Visível" : "Oculto")}");
            }

            // Atualizar fórmula
            if (_atomCounts.Count == 0)
            {
                FormulaText.Text = "Vazio";
            }
            else
            {
                FormulaText.Text = GenerateFormula();
                System.Diagnostics.Debug.WriteLine($"📝 Fórmula: {FormulaText.Text}");
            }
        }

        /// <summary>
        /// Gera a fórmula química (ex: H₂O, CH₄, NaOH)
        /// Segue convenções IUPAC:
        /// - Compostos orgânicos: C, H, depois alfabética (CH₄, C₂H₆O)
        /// - Hidróxidos: Metal + OH (NaOH, Ca(OH)₂)
        /// - Ácidos: H + não-metal (HCl, H₂SO₄)
        /// - Sais: Metal + não-metal (NaCl, CaCO₃)
        /// </summary>
        private string GenerateFormula()
        {
            var formula = "";

            // Detectar tipo de composto
            bool hasCarbon = _atomCounts.ContainsKey("C");
            bool hasHydrogen = _atomCounts.ContainsKey("H");
            bool hasOxygen = _atomCounts.ContainsKey("O");
            bool hasMetal = _atomCounts.Keys.Any(IsMetal);

            // CASO 1: Hidróxido (Metal + O + H) → NaOH, Ca(OH)₂
            if (hasMetal && hasOxygen && hasHydrogen && !hasCarbon)
            {
                // Metal primeiro
                foreach (var metal in _atomCounts.Where(x => IsMetal(x.Key)).OrderBy(x => x.Key))
                {
                    formula += metal.Key;
                    if (metal.Value > 1)
                        formula += ConvertToSubscript(metal.Value);
                }

                // Depois OH
                formula += "OH";
                return formula;
            }

            // CASO 2: Ácido (H + não-metal) → HCl, H₂SO₄
            if (hasHydrogen && !hasMetal && !hasCarbon)
            {
                // H primeiro
                formula += "H";
                if (_atomCounts["H"] > 1)
                    formula += ConvertToSubscript(_atomCounts["H"]);

                // Depois outros elementos em ordem alfabética
                foreach (var kvp in _atomCounts.Where(x => x.Key != "H").OrderBy(x => x.Key))
                {
                    formula += kvp.Key;
                    if (kvp.Value > 1)
                        formula += ConvertToSubscript(kvp.Value);
                }
                return formula;
            }

            // CASO 3: Composto orgânico (C, H) → CH₄, C₂H₆O
            if (hasCarbon)
            {
                // Ordem: C, H, depois alfabética
                var ordered = _atomCounts.OrderBy(x => GetOrganicPriority(x.Key));

                foreach (var kvp in ordered)
                {
                    formula += kvp.Key;
                    if (kvp.Value > 1)
                        formula += ConvertToSubscript(kvp.Value);
                }
                return formula;
            }

            // CASO 4: Sal ou composto genérico (Metal + não-metal) → NaCl, CaCO₃
            if (hasMetal)
            {
                // Metal primeiro
                foreach (var metal in _atomCounts.Where(x => IsMetal(x.Key)).OrderBy(x => x.Key))
                {
                    formula += metal.Key;
                    if (metal.Value > 1)
                        formula += ConvertToSubscript(metal.Value);
                }

                // Depois não-metais
                foreach (var nonMetal in _atomCounts.Where(x => !IsMetal(x.Key)).OrderBy(x => x.Key))
                {
                    formula += nonMetal.Key;
                    if (nonMetal.Value > 1)
                        formula += ConvertToSubscript(nonMetal.Value);
                }
                return formula;
            }

            // CASO 5: Outros (ordem alfabética)
            var defaultOrdered = _atomCounts.OrderBy(x => x.Key);
            foreach (var kvp in defaultOrdered)
            {
                formula += kvp.Key;
                if (kvp.Value > 1)
                    formula += ConvertToSubscript(kvp.Value);
            }

            return formula;
        }

        /// <summary>
        /// Prioridade para compostos orgânicos (C > H > alfabética)
        /// </summary>
        private int GetOrganicPriority(string symbol)
        {
            return symbol switch
            {
                "C" => 0,
                "H" => 1,
                _ => 2
            };
        }

        /// <summary>
        /// Retorna átomos ordenados pela mesma lógica da fórmula
        /// </summary>
        private IOrderedEnumerable<KeyValuePair<string, int>> GetOrderedAtoms()
        {
            bool hasCarbon = _atomCounts.ContainsKey("C");
            bool hasMetal = _atomCounts.Keys.Any(IsMetal);

            // Compostos orgânicos: C, H, alfabética
            if (hasCarbon)
            {
                return _atomCounts.OrderBy(x => GetOrganicPriority(x.Key));
            }

            // Compostos com metal: metal primeiro, depois alfabética
            if (hasMetal)
            {
                return _atomCounts.OrderBy(x => IsMetal(x.Key) ? 0 : 1).ThenBy(x => x.Key);
            }

            // Padrão: alfabética
            return _atomCounts.OrderBy(x => x.Key);
        }

        /// <summary>
        /// Verifica se é um metal
        /// </summary>
        private bool IsMetal(string symbol)
        {
            var metals = new[] { "Na", "K", "Ca", "Mg", "Fe", "Cu", "Zn", "Ag", "Al", "Li", "Ba", "Sr" };
            return metals.Contains(symbol);
        }

        /// <summary>
        /// Converte número para subscrito Unicode
        /// </summary>
        private string ConvertToSubscript(int number)
        {
            var subscripts = new[] { "₀", "₁", "₂", "₃", "₄", "₅", "₆", "₇", "₈", "₉" };
            var result = "";
            foreach (var digit in number.ToString())
            {
                result += subscripts[digit - '0'];
            }
            return result;
        }

        /// <summary>
        /// Cores CPK para elementos
        /// </summary>
        private Color GetElementColor(string element)
        {
            return element switch
            {
                "H" => Color.FromRgb(255, 255, 255),  // Branco
                "C" => Color.FromRgb(144, 144, 144),  // Cinza
                "N" => Color.FromRgb(48, 80, 248),    // Azul
                "O" => Color.FromRgb(255, 13, 13),    // Vermelho
                "F" => Color.FromRgb(144, 224, 80),   // Verde claro
                "Cl" => Color.FromRgb(31, 240, 31),   // Verde
                "Br" => Color.FromRgb(166, 41, 41),   // Marrom
                "I" => Color.FromRgb(148, 0, 148),    // Roxo
                "S" => Color.FromRgb(255, 255, 48),   // Amarelo
                "P" => Color.FromRgb(255, 128, 0),    // Laranja
                "Na" => Color.FromRgb(171, 92, 242),  // Violeta
                "K" => Color.FromRgb(143, 64, 212),   // Violeta escuro
                "Ca" => Color.FromRgb(61, 255, 0),    // Verde limão
                "Mg" => Color.FromRgb(138, 255, 0),   // Verde amarelado
                "Fe" => Color.FromRgb(224, 102, 51),  // Laranja escuro
                "Cu" => Color.FromRgb(200, 128, 51),  // Cobre
                "Zn" => Color.FromRgb(125, 128, 176), // Cinza azulado
                "Ag" => Color.FromRgb(192, 192, 192), // Prata
                "Al" => Color.FromRgb(191, 166, 166), // Cinza rosado
                "Si" => Color.FromRgb(240, 200, 160), // Bege
                _ => Color.FromRgb(255, 20, 147)      // Rosa (desconhecido)
            };
        }

        /// <summary>
        /// Calcula massa molar aproximada
        /// </summary>
        private double CalculateMolarMass()
        {
            double mass = 0;
            foreach (var kvp in _atomCounts)
            {
                var element = _availableElements.First(e => e.Symbol == kvp.Key);
                mass += element.AtomicMass * kvp.Value;
            }
            return mass;
        }

        // ========== EVENT HANDLERS ==========

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (_atomCounts.Count == 0)
            {
                MessageBox.Show("Adicione pelo menos um átomo à molécula!", 
                               "Molécula Vazia", 
                               MessageBoxButton.OK, 
                               MessageBoxImage.Warning);
                return;
            }

            // Criar composto
            CreatedCompound = new Compound
            {
                Name = string.IsNullOrWhiteSpace(MoleculeNameText.Text) 
                    ? "Composto Customizado" 
                    : MoleculeNameText.Text,
                Formula = GenerateFormula(),
                MolarMass = CalculateMolarMass(),
                MeltingPoint = 273.15,  // Padrão: 0°C
                BoilingPoint = 373.15   // Padrão: 100°C
            };

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _atomCounts.Clear();
            UpdateMoleculeView();
        }
    }

    /// <summary>
    /// ViewModel para visualização de átomos
    /// </summary>
    public class AtomViewModel : INotifyPropertyChanged
    {
        private string _symbol = "";
        private int _count;
        private SolidColorBrush _color = new SolidColorBrush(Colors.White);
        private Visibility _countVisibility = Visibility.Collapsed;

        public string Symbol
        {
            get => _symbol;
            set
            {
                _symbol = value;
                OnPropertyChanged(nameof(Symbol));
            }
        }

        public int Count
        {
            get => _count;
            set
            {
                _count = value;
                OnPropertyChanged(nameof(Count));
                CountVisibility = value > 1 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public SolidColorBrush Color
        {
            get => _color;
            set
            {
                _color = value;
                OnPropertyChanged(nameof(Color));
            }
        }

        public Visibility CountVisibility
        {
            get => _countVisibility;
            set
            {
                _countVisibility = value;
                OnPropertyChanged(nameof(CountVisibility));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
