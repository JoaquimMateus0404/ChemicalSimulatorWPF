using System.Windows.Controls;
using System.Windows.Input;
using ChemicalSimulator.ViewModels;

namespace ChemicalSimulator.Views
{
    public partial class EducationalView : UserControl
    {
        public EducationalView()
        {
            InitializeComponent();
            
            // Habilitar navegação por teclado
            this.KeyDown += EducationalView_KeyDown;
            this.Focusable = true;
            this.Loaded += (s, e) => this.Focus();
        }

        private void EducationalView_KeyDown(object sender, KeyEventArgs e)
        {
            // TODO: Criar EducationalViewModel
            /*
            if (DataContext is not EducationalViewModel viewModel) return;

            switch (e.Key)
            {
                case Key.Right:
                case Key.PageDown:
                    if (viewModel.NextStepCommand.CanExecute(null))
                    {
                        viewModel.NextStepCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;

                case Key.Left:
                case Key.PageUp:
                    if (viewModel.PreviousStepCommand.CanExecute(null))
                    {
                        viewModel.PreviousStepCommand.Execute(null);
                        e.Handled = true;
                    }
                    break;

                case Key.Home:
                    viewModel.ResetProgressCommand.Execute(null);
                    e.Handled = true;
                    break;
            }
            */
        }
    }
}
