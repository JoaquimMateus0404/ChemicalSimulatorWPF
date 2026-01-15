namespace ChemicalSimulator.Models;

public class ReactionConditions
{
    public double Temperature { get; set; } = 298.15; // K (25°C)
    public double Pressure { get; set; } = 101.325; // kPa (1 atm)
    public string Solvent { get; set; } = "None";
    public string Catalyst { get; set; } = "None";
    public double pH { get; set; } = 7.0;
    public bool IsAcidic => pH < 7;
    public bool IsBasic => pH > 7;
}
