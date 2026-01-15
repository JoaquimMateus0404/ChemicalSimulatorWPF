namespace ChemicalSimulator.Models;

public class ReactionComponent
{
    public Molecule Molecule { get; set; }
    public int Coefficient { get; set; }
    public string Phase { get; set; } // (s), (l), (g), (aq)
}
