using System.Windows.Controls;
using ChemicalSimulator.ViewModels;

namespace ChemicalSimulator.Views
{
    /// <summary>
    /// Interaction logic for CompoundLibraryView.xaml
    /// </summary>
    public partial class CompoundLibraryView : UserControl
    {
        public CompoundLibraryView()
        {
            InitializeComponent();
            //DataContext = new CompoundLibraryViewModel();
        }
    }
}
