namespace ChemicalSimulator.Models;

/// <summary>
/// Representa uma reação química completa
/// </summary>
public class Reaction
{
    public List<ReactionComponent> Reactants { get; set; }
    public List<ReactionComponent> Products { get; set; }
    public ReactionConditions Conditions { get; set; }
    public ReactionType Type { get; set; }

    // Termodinâmica
    public double EnthalpyChange { get; set; } // ΔH (kJ/mol)
    public double EntropyChange { get; set; } // ΔS (J/mol·K)
    public double GibbsFreeEnergy { get; set; } // ΔG (kJ/mol)
    public double ActivationEnergy { get; set; } // Ea (kJ/mol)

    // Cinética
    public double RateConstant { get; set; } // k
    public int ReactionOrder { get; set; }
    public double HalfLife { get; set; }

    // Estado
    public bool IsBalanced { get; set; }
    public bool IsSpontaneous => GibbsFreeEnergy < 0;
    public bool IsExothermic => EnthalpyChange < 0;

    public Reaction()
    {
        Reactants = new List<ReactionComponent>();
        Products = new List<ReactionComponent>();
        Conditions = new ReactionConditions();
    }

    public void CalculateThermodynamics()
    {
        // ΔH = Σ(ΔHf produtos) - Σ(ΔHf reagentes)
        double productsEnthalpy = 0;
        double reactantsEnthalpy = 0;

        foreach (var p in Products)
            productsEnthalpy += p.Coefficient * p.Molecule.EnthalpyOfFormation;

        foreach (var r in Reactants)
            reactantsEnthalpy += r.Coefficient * r.Molecule.EnthalpyOfFormation;

        EnthalpyChange = productsEnthalpy - reactantsEnthalpy;

        // ΔG = ΔH - TΔS
        GibbsFreeEnergy = EnthalpyChange -
            (Conditions.Temperature * EntropyChange / 1000.0);
    }

    public double CalculateReactionRate()
    {
        // Lei de Arrhenius: k = A * e^(-Ea/RT)
        const double R = 8.314; // J/(mol·K)
        double A = 1e13; // Fator pré-exponencial típico

        RateConstant = A * Math.Exp(-ActivationEnergy * 1000 /
            (R * Conditions.Temperature));

        return RateConstant;
    }
}

public enum ReactionType
{
    Synthesis,
    Decomposition,
    SingleDisplacement,
    DoubleDisplacement,
    Combustion,
    Redox,
    AcidBase,
    Precipitation
}