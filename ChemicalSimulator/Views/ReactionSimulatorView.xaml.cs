using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ChemicalSimulator.ViewModels;
using ChemicalSimulator.Models;

namespace ChemicalSimulator.Views
{
    /// <summary>
    /// Code-behind para ReactionSimulatorView com Animação 3D
    /// </summary>
    public partial class ReactionSimulatorView : UserControl
    {
        private ReactionSimulatorViewModel? ViewModel => DataContext as ReactionSimulatorViewModel;

        public ReactionSimulatorView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ReactionSimulatorViewModel oldVm)
            {
                // Desconectar eventos antigos
                oldVm.PropertyChanged -= ViewModel_PropertyChanged;
            }

            if (e.NewValue is ReactionSimulatorViewModel newVm)
            {
                // Conectar eventos novos
                newVm.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ViewModel == null) return;

            switch (e.PropertyName)
            {
                case nameof(ReactionSimulatorViewModel.AnimationProgress):
                    // Atualizar texto da animação 3D
                    MolecularAnimation3D.UpdateAnimationStep(ViewModel.AnimationStepDescription);
                    break;

                case nameof(ReactionSimulatorViewModel.IsAnimating):
                    // Iniciar ou parar animação 3D
                    if (ViewModel.IsAnimating)
                    {
                        double speed = GetAnimationSpeedValue(ViewModel.AnimationSpeed);
                        MolecularAnimation3D.StartAnimation(speed);
                    }
                    else
                    {
                        MolecularAnimation3D.StopAnimation();
                    }
                    break;

                case nameof(ReactionSimulatorViewModel.CurrentReaction):
                    // Atualizar moléculas 3D quando reação mudar
                    if (ViewModel.CurrentReaction != null)
                    {
                        MolecularAnimation3D.SetReaction(ViewModel.CurrentReaction);
                    }
                    break;

                case "ResetAnimation":
                    // Resetar posições 3D
                    MolecularAnimation3D.ResetPositions();
                    break;
            }
        }

        private double GetAnimationSpeedValue(string speedName)
        {
            return speedName switch
            {
                "Lenta" => 0.5,
                "Normal" => 1.0,
                "Rápida" => 2.0,
                "Instantânea" => 100.0,
                _ => 1.0
            };
        }

        // ========== MOLECULE BUILDER HANDLERS ==========

        private void BuildReactant1_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;

            var dialog = new MoleculeBuilderDialog(ViewModel.AvailableElements.ToList())
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true && dialog.CreatedCompound != null)
            {
                // Adicionar ao AvailableCompounds temporariamente
                ViewModel.AvailableCompounds.Add(dialog.CreatedCompound);
                
                // Selecionar automaticamente
                ViewModel.SelectedReactant1 = dialog.CreatedCompound;

                System.Diagnostics.Debug.WriteLine($"✅ Molécula criada: {dialog.CreatedCompound.Formula} ({dialog.CreatedCompound.Name})");
            }
        }

        private void BuildReactant2_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;

            var dialog = new MoleculeBuilderDialog(ViewModel.AvailableElements.ToList())
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true && dialog.CreatedCompound != null)
            {
                // Adicionar ao AvailableCompounds temporariamente
                ViewModel.AvailableCompounds.Add(dialog.CreatedCompound);
                
                // Selecionar automaticamente
                ViewModel.SelectedReactant2 = dialog.CreatedCompound;

                System.Diagnostics.Debug.WriteLine($"✅ Molécula criada: {dialog.CreatedCompound.Formula} ({dialog.CreatedCompound.Name})");
            }
        }
    }
}


