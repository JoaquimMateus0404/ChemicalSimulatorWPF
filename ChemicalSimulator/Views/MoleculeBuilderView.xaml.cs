using System.Windows.Controls;
using ChemicalSimulator.ViewModels;

namespace ChemicalSimulator.Views
{
    public partial class MoleculeBuilderView : UserControl
    {
        public MoleculeBuilderView()
        {
            InitializeComponent();
            DataContext = new MoleculeBuilderViewModel();
        }
    }
}