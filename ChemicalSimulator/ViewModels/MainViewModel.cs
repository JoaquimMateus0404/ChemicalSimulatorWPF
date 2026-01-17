using System.Collections.ObjectModel;
using System.Windows.Input;
using ChemicalSimulator.Commands;
using ChemicalSimulator.Models;
using ChemicalSimulator.Services;

namespace ChemicalSimulator.ViewModels
{
    /// <summary>
    /// ViewModel principal da aplicação
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentView;
        private string _currentViewName = "Construtor de Moléculas";

        private readonly ElementDataLoader _elementLoader;
        private readonly ChemistryEngine _chemistryEngine;

        public ObservableCollection<Element> AvailableElements { get; }
        public ObservableCollection<Molecule> SavedMolecules { get; }

        public ViewModelBase CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public string CurrentViewName
        {
            get => _currentViewName;
            set => SetProperty(ref _currentViewName, value);
        }

        // Comandos de navegação
        public ICommand NavigateToMoleculeBuilderCommand { get; }
        public ICommand NavigateToReactionSimulatorCommand { get; }
        public ICommand NavigateToEducationalModeCommand { get; }
        public ICommand NavigateToPeriodicTableCommand { get; }
        public ICommand NavigateToCompoundLibraryCommand { get; }

        public MainViewModel()
        {
            _elementLoader = new ElementDataLoader();
            _chemistryEngine = new ChemistryEngine();

            AvailableElements = new ObservableCollection<Element>();
            SavedMolecules = new ObservableCollection<Molecule>();

            // Inicializar comandos
            NavigateToMoleculeBuilderCommand = new RelayCommand(NavigateToMoleculeBuilder);
            NavigateToReactionSimulatorCommand = new RelayCommand(NavigateToReactionSimulator);
            NavigateToEducationalModeCommand = new RelayCommand(NavigateToEducationalMode);
            NavigateToPeriodicTableCommand = new RelayCommand(NavigateToPeriodicTable);
            NavigateToCompoundLibraryCommand = new RelayCommand(NavigateToCompoundLibrary);

            // Carregar dados
            LoadElements();

            // Definir view inicial
            NavigateToMoleculeBuilder();
        }

        private async void LoadElements()
        {
            try
            {
                await Task.Run(() =>
                {
                    var elements = _elementLoader.LoadElements();
                    System.Diagnostics.Debug.WriteLine($"🔍 Total de elementos carregados: {elements.Count}");
                    
                    foreach (var element in elements)
                    {
                        AvailableElements.Add(element);
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"✅ {AvailableElements.Count} elementos adicionados à coleção");
                    
                    // Diagnóstico por categoria
                    var categories = elements.GroupBy(e => e.Category)
                                            .Select(g => $"{g.Key}: {g.Count()}")
                                            .ToList();
                    System.Diagnostics.Debug.WriteLine($"📊 Distribuição por categoria:");
                    foreach (var cat in categories)
                    {
                        System.Diagnostics.Debug.WriteLine($"   {cat}");
                    }
                });
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao carregar elementos: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        private void NavigateToMoleculeBuilder()
        {
            CurrentView = new MoleculeBuilderViewModel(AvailableElements, SavedMolecules);
            CurrentViewName = "🔬 Construtor de Moléculas";
        }

        private void NavigateToReactionSimulator()
        {
            CurrentView = new ReactionSimulatorViewModel(SavedMolecules);
            CurrentViewName = "⚗️ Simulador de Reações";
        }

        private void NavigateToEducationalMode()
        {
            CurrentView = new EducationalViewModel();
            CurrentViewName = "📚 Modo Educacional";
        }

        private void NavigateToPeriodicTable()
        {
            CurrentView = new PeriodicTableViewModel(AvailableElements);
            CurrentViewName = "🧪 Tabela Periódica";
        }

        private void NavigateToCompoundLibrary()
        {
            CurrentView = new CompoundLibraryViewModel();
            CurrentViewName = "📖 Biblioteca de Compostos";
        }
    }
}
