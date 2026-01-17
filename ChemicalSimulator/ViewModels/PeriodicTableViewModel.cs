using System.Collections.ObjectModel;
using System.Windows.Input;
using ChemicalSimulator.Commands;
using ChemicalSimulator.Models;

namespace ChemicalSimulator.ViewModels
{
    /// <summary>
    /// ViewModel para exibição da tabela periódica
    /// </summary>
    public class PeriodicTableViewModel : ViewModelBase
    {
        private Element? _selectedElement;
        private string _searchText = "";
        private bool _showLegend = true;
        private string _filterCategory = "All";

        public ObservableCollection<Element> Elements { get; }
        public ObservableCollection<Element> FilteredElements { get; }

        public Element? SelectedElement
        {
            get => _selectedElement;
            set
            {
                SetProperty(ref _selectedElement, value);
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                FilterElements();
            }
        }

        public bool ShowLegend
        {
            get => _showLegend;
            set => SetProperty(ref _showLegend, value);
        }

        public string FilterCategory
        {
            get => _filterCategory;
            set
            {
                SetProperty(ref _filterCategory, value);
                FilterElements();
            }
        }

        public ICommand SelectElementCommand { get; }
        public ICommand ClearSelectionCommand { get; }
        public ICommand FilterByCategoryCommand { get; }

        public PeriodicTableViewModel(ObservableCollection<Element> elements)
        {
            Elements = elements;
            FilteredElements = new ObservableCollection<Element>(elements);

            SelectElementCommand = new RelayCommand<Element>(element => SelectedElement = element);
            ClearSelectionCommand = new RelayCommand(() => SelectedElement = null);
            FilterByCategoryCommand = new RelayCommand<string>(category => FilterCategory = category ?? "All");
        }

        private void FilterElements()
        {
            FilteredElements.Clear();

            var filtered = Elements.AsEnumerable();

            // Filtro por texto
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(e =>
                    e.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    e.Symbol.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    e.AtomicNumber.ToString().Contains(SearchText));
            }

            // Filtro por categoria
            if (FilterCategory != "All")
            {
                filtered = filtered.Where(e => e.Category.ToString() == FilterCategory);
            }

            foreach (var element in filtered)
            {
                FilteredElements.Add(element);
            }
        }

        /// <summary>
        /// Obtém todos os elementos organizados por posição na tabela periódica
        /// </summary>
        public Dictionary<(int Row, int Column), Element> GetElementPositions()
        {
            var positions = new Dictionary<(int Row, int Column), Element>();

            foreach (var element in Elements)
            {
                int row = element.Period;
                int column = element.Group ?? 0; // Usar 0 se Group for null

                // Ajustes especiais para lantanídeos e actinídeos
                if (element.Category == ElementCategory.Lanthanide)
                {
                    row = 9; // Linha especial para lantanídeos
                    column = element.AtomicNumber - 57 + 3;
                }
                else if (element.Category == ElementCategory.Actinide)
                {
                    row = 10; // Linha especial para actinídeos
                    column = element.AtomicNumber - 89 + 3;
                }
                else if (column == 0)
                {
                    // Pular elementos sem grupo definido que não são lantanídeos/actinídeos
                    continue;
                }

                positions[(row, column)] = element;
            }

            return positions;
        }
    }
}
