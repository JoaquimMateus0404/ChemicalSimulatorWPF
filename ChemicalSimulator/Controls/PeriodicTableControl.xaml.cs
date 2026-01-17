using System.Windows;
using System.Windows.Controls;
using ChemicalSimulator.Models;
using ChemicalSimulator.ViewModels;

namespace ChemicalSimulator.Controls
{
    public partial class PeriodicTableControl : UserControl
    {
        public PeriodicTableControl()
        {
            InitializeComponent();
        }

        // Dependency Property para o elemento selecionado
        public static readonly DependencyProperty SelectedElementProperty =
            DependencyProperty.Register(
                nameof(SelectedElement),
                typeof(Element),
                typeof(PeriodicTableControl),
                new PropertyMetadata(null, OnSelectedElementChanged));

        public Element SelectedElement
        {
            get => (Element)GetValue(SelectedElementProperty);
            set => SetValue(SelectedElementProperty, value);
        }

        private static void OnSelectedElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PeriodicTableControl)d;
            control.OnElementSelected(e.NewValue as Element);
        }

        private void OnElementSelected(Element element)
        {
            // Evento pode ser usado pelo ViewModel
            ElementSelected?.Invoke(this, new ElementSelectedEventArgs(element));
        }

        public event EventHandler<ElementSelectedEventArgs> ElementSelected;
    }

    public class ElementSelectedEventArgs : EventArgs
    {
        public Element Element { get; }

        public ElementSelectedEventArgs(Element element)
        {
            Element = element;
        }
    }
}