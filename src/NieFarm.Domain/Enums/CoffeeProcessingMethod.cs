namespace NieFarm.Domain.Enums;

/// <summary>How the cherries were processed (R-26). Stored as int; never renumber.</summary>
public enum CoffeeProcessingMethod
{
    Washed = 1,
    Natural = 2,
    Honey = 3,
    SemiWashed = 4,
    Anaerobic = 5,
    Other = 99
}
