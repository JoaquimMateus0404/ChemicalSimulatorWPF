using System.Windows;
using ChemicalSimulator.ViewModels;

namespace ChemicalSimulator.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}