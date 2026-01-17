using System;
using System.Collections.Generic;

namespace ChemicalSimulator.Models
{
    /// <summary>
    /// Representa uma ação no histórico (para Undo/Redo)
    /// </summary>
    public class MoleculeAction
    {
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public ActionType Type { get; set; }
        public object? Data { get; set; }
        
        // Propriedades específicas para átomos e ligações
        public Atom? Atom { get; set; }
        public Bond? Bond { get; set; }
        public List<Bond> RelatedBonds { get; set; } = new List<Bond>();
        
        public MoleculeAction() { }
        
        public MoleculeAction(string description, ActionType type, object? data = null)
        {
            Description = description;
            Type = type;
            Data = data;
        }
    }
    
    public enum ActionType
    {
        AddAtom,
        RemoveAtom,
        AddBond,
        RemoveBond,
        OptimizeGeometry,
        Clear,
        LoadTemplate
    }
}
