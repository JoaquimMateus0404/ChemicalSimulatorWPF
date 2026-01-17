using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ChemicalSimulator.Commands;
using ChemicalSimulator.Models;
using ChemicalSimulator.Services;

using OxyPlot;
using OxyPlot.Series;

namespace ChemicalSimulator.ViewModels
{
    /// <summary>
    /// ViewModel AVANÇADA para simulação profissional de reações químicas
    /// Com funcionalidades de predição, análise termodinâmica e cinética em tempo real
    /// </summary>
    public partial class ReactionSimulatorViewModel : ViewModelBase
    {
        private readonly ReactionPredictor _reactionPredictor;
        private readonly ChemistryEngine _chemistryEngine;
        private readonly ElementDataLoader _elementDataLoader;
        private readonly CompoundDataLoader _compoundDataLoader;


       
       

       
       
       
    }
}
