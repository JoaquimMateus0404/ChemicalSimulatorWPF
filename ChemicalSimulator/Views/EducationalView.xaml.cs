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
        }

        private void TopicItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item && item.Content is LessonTopic topic)
            {
                var viewModel = (EducationalViewModel)DataContext;
                viewModel.SelectTopicCommand.Execute(topic);
            }
        }
    }
}
