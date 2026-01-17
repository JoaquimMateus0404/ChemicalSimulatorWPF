using System.Windows.Controls;
using ChemicalSimulator.ViewModels;

namespace ChemicalSimulator.Views
{
    public partial class SimulationView : UserControl
    {
        public SimulationView()
        {
            InitializeComponent();
            DataContext = new SimulationViewModel();
        }
    }
}