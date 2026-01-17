using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ChemicalSimulator.Commands;
using ChemicalSimulator.Models;
using ChemicalSimulator.Services;

namespace ChemicalSimulator.ViewModels
{
    /// <summary>
    /// ViewModel para a biblioteca de compostos químicos comuns
    /// </summary>
    public class CompoundLibraryViewModel : ViewModelBase
    {
        private readonly CompoundDataLoader _compoundLoader;
        
        private ObservableCollection<Compound> _compounds;
        private ObservableCollection<Compound> _filteredCompounds;
        private Compound? _selectedCompound;
        private string _searchText = string.Empty;
        private CompoundCategory? _selectedCategory;
        private PhysicalState? _selectedState;
        private string _sortBy = "Name";
        private bool _isAscending = true;

        public CompoundLibraryViewModel()
        {
            _compoundLoader = new CompoundDataLoader();
            
            // Inicializar coleções
            _compounds = new ObservableCollection<Compound>();
            _filteredCompounds = new ObservableCollection<Compound>();
            
            // Comandos
            SearchCommand = new RelayCommand(ExecuteSearch);
            ClearSearchCommand = new RelayCommand(ExecuteClearSearch);
            FilterByCategoryCommand = new RelayCommand<CompoundCategory?>(ExecuteFilterByCategory);
            FilterByStateCommand = new RelayCommand<PhysicalState?>(ExecuteFilterByState);
            SortCommand = new RelayCommand<string>(ExecuteSort);
            SelectCompoundCommand = new RelayCommand<Compound>(ExecuteSelectCompound);
            
            // Carregar compostos
            LoadCompounds();
        }

        #region Propriedades

        public ObservableCollection<Compound> Compounds
        {
            get => _compounds;
            set => SetProperty(ref _compounds, value);
        }

        public ObservableCollection<Compound> FilteredCompounds
        {
            get => _filteredCompounds;
            set => SetProperty(ref _filteredCompounds, value);
        }

        public Compound? SelectedCompound
        {
            get => _selectedCompound;
            set => SetProperty(ref _selectedCompound, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilters();
                }
            }
        }

        public CompoundCategory? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    ApplyFilters();
                }
            }
        }

        public PhysicalState? SelectedState
        {
            get => _selectedState;
            set
            {
                if (SetProperty(ref _selectedState, value))
                {
                    ApplyFilters();
                }
            }
        }

        public string SortBy
        {
            get => _sortBy;
            set => SetProperty(ref _sortBy, value);
        }

        public bool IsAscending
        {
            get => _isAscending;
            set => SetProperty(ref _isAscending, value);
        }

        public int TotalCompounds => Compounds.Count;
        public int FilteredCount => FilteredCompounds.Count;

        #endregion

        #region Comandos

        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand FilterByCategoryCommand { get; }
        public ICommand FilterByStateCommand { get; }
        public ICommand SortCommand { get; }
        public ICommand SelectCompoundCommand { get; }

        #endregion

        #region Métodos Privados

        private void LoadCompounds()
        {
            try
            {
                var compounds = _compoundLoader.LoadCompounds();
                
                Compounds.Clear();
                foreach (var compound in compounds)
                {
                    Compounds.Add(compound);
                }
                
                ApplyFilters();
                
                System.Diagnostics.Debug.WriteLine($"✅ {compounds.Count} compostos carregados na biblioteca");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Erro ao carregar biblioteca de compostos: {ex.Message}");
            }
        }

        private void ApplyFilters()
        {
            var filtered = Compounds.AsEnumerable();

            // Filtro de busca
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(c =>
                    c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    c.Formula.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (c.CommonUse?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            // Filtro de categoria
            if (SelectedCategory.HasValue)
            {
                filtered = filtered.Where(c => c.Category == SelectedCategory.Value);
            }

            // Filtro de estado físico
            if (SelectedState.HasValue)
            {
                filtered = filtered.Where(c => c.State == SelectedState.Value);
            }

            // Ordenação
            filtered = ApplySort(filtered);

            // Atualizar coleção filtrada
            FilteredCompounds.Clear();
            foreach (var compound in filtered)
            {
                FilteredCompounds.Add(compound);
            }

            OnPropertyChanged(nameof(FilteredCount));
        }

        private IEnumerable<Compound> ApplySort(IEnumerable<Compound> compounds)
        {
            return SortBy switch
            {
                "Name" => IsAscending 
                    ? compounds.OrderBy(c => c.Name) 
                    : compounds.OrderByDescending(c => c.Name),
                    
                "Formula" => IsAscending 
                    ? compounds.OrderBy(c => c.Formula) 
                    : compounds.OrderByDescending(c => c.Formula),
                    
                "MolarMass" => IsAscending 
                    ? compounds.OrderBy(c => c.MolarMass) 
                    : compounds.OrderByDescending(c => c.MolarMass),
                    
                "Category" => IsAscending 
                    ? compounds.OrderBy(c => c.Category) 
                    : compounds.OrderByDescending(c => c.Category),
                    
                "State" => IsAscending 
                    ? compounds.OrderBy(c => c.State) 
                    : compounds.OrderByDescending(c => c.State),
                    
                _ => compounds
            };
        }

        private void ExecuteSearch()
        {
            ApplyFilters();
        }

        private void ExecuteClearSearch()
        {
            SearchText = string.Empty;
            SelectedCategory = null;
            SelectedState = null;
        }

        private void ExecuteFilterByCategory(CompoundCategory? category)
        {
            SelectedCategory = category;
        }

        private void ExecuteFilterByState(PhysicalState? state)
        {
            SelectedState = state;
        }

        private void ExecuteSort(string? sortBy)
        {
            if (string.IsNullOrEmpty(sortBy))
                return;

            if (SortBy == sortBy)
            {
                // Alternar direção
                IsAscending = !IsAscending;
            }
            else
            {
                // Novo critério
                SortBy = sortBy;
                IsAscending = true;
            }

            ApplyFilters();
        }

        private void ExecuteSelectCompound(Compound? compound)
        {
            SelectedCompound = compound;
        }

        #endregion

        #region Métodos Públicos

        /// <summary>
        /// Obtém estatísticas dos compostos
        /// </summary>
        public Dictionary<string, int> GetStatistics()
        {
            var stats = new Dictionary<string, int>
            {
                ["Total"] = Compounds.Count,
                ["Sólidos"] = Compounds.Count(c => c.State == PhysicalState.Solid),
                ["Líquidos"] = Compounds.Count(c => c.State == PhysicalState.Liquid),
                ["Gasosos"] = Compounds.Count(c => c.State == PhysicalState.Gas),
                ["Ácidos"] = Compounds.Count(c => c.Category == CompoundCategory.Acid),
                ["Bases"] = Compounds.Count(c => c.Category == CompoundCategory.Base),
                ["Sais"] = Compounds.Count(c => c.Category == CompoundCategory.Salt),
                ["Óxidos"] = Compounds.Count(c => c.Category == CompoundCategory.Oxide),
                ["Orgânicos"] = Compounds.Count(c => c.Category == CompoundCategory.Organic),
                ["Inorgânicos"] = Compounds.Count(c => c.Category == CompoundCategory.Inorganic)
            };

            return stats;
        }

        /// <summary>
        /// Recarrega a biblioteca de compostos
        /// </summary>
        public void Reload()
        {
            _compoundLoader.ClearCache();
            LoadCompounds();
        }

        #endregion
    }
}
